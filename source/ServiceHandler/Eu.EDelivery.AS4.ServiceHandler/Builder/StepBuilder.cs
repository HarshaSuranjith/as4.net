﻿using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;

namespace Eu.EDelivery.AS4.ServiceHandler.Builder
{
    /// <summary>
    /// Builder to make <see cref="IStep"/> implementation
    /// from <see cref="Step"/> settings
    /// </summary>
    public class StepBuilder
    {
        private Model.Internal.Steps _settingSteps;

        /// <summary>
        /// Set the configured <see cref="Model.Internal.Steps"/> settings
        /// </summary>
        /// <param name="settingSteps"></param>
        /// <returns></returns>
        public StepBuilder SetSettings(Model.Internal.Steps settingSteps)
        {
            this._settingSteps = settingSteps;
            return this;
        }

        /// <summary>
        /// Build the <see cref="IStep"/> implementation
        /// </summary>
        /// <returns></returns>
        public IStep Build()
        {
            IStep decoratedStep = CreateDecoratorStep(this._settingSteps);
            IList<IStep> unDecoratedSteps = CreateUndecoratedSteps();

            if (unDecoratedSteps.Count == 0)
                return decoratedStep;

            unDecoratedSteps.Insert(0, decoratedStep);
            return new CompositeStep(unDecoratedSteps.ToArray());
        }

        
        private IStep CreateDecoratorStep(Model.Internal.Steps settingsSteps)
        {
            IStep[] decoratedSteps = settingsSteps.Step
                .Where(s => s.UnDecorated == false)
                .Select(CreateInstance)
                .ToArray();

            var compositeStep = new CompositeStep(decoratedSteps);
            return settingsSteps.Decorator != null
                ? CreateInstance<IStep>(settingsSteps.Decorator, compositeStep)
                : compositeStep;
        }

        private IStep CreateInstance(Step settingStep)
        {
            return settingStep.Setting != null
                ? CreateConfigurableStep(settingStep)
                : CreateInstance<IStep>(settingStep.Type);
        }

        private IConfigStep CreateConfigurableStep(Step settingStep)
        {
            var step = CreateInstance<IConfigStep>(settingStep.Type);

            Dictionary<string, string> dictionary = settingStep.Setting
                .ToDictionary(setting => setting.Key, setting => setting.Value);

            step.Configure(dictionary);

            return step;
        }

        private IList<IStep> CreateUndecoratedSteps()
        {
            return this._settingSteps.Step
                .Where(s => s.UnDecorated == true)
                .Select(settingStep => CreateInstance<IStep>(settingStep.Type))
                .ToList();
        }

        private T CreateInstance<T>(string typeString, params object[] args) where T : class
        {
            return new GenericTypeBuilder().SetType(typeString).SetArgs(args).Build<T>();
        }
    }
}