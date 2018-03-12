﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Transformers;
using NLog;

namespace Eu.EDelivery.AS4.Agents
{
    public class Agent : IAgent
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IReceiver _receiver;
        private readonly Transformer _transformerConfig;
        private readonly IAgentExceptionHandler _exceptionHandler;
        private readonly (ConditionalStepConfig happyPath, ConditionalStepConfig unhappyPath) _conditionalPipeline;
        private readonly StepConfiguration _stepConfiguration;

        private Agent(
            string name,
            IReceiver receiver,
            Transformer transformerConfig,
            IAgentExceptionHandler exceptionHandler)
        {
            _receiver = receiver;
            _transformerConfig = transformerConfig;
            _exceptionHandler = exceptionHandler;

            AgentConfig = new AgentConfig(name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="transformerConfig">The transformer configuration.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="stepConfiguration">The step configuration.</param>
        internal Agent(
            string name,
            IReceiver receiver,
            Transformer transformerConfig,
            IAgentExceptionHandler exceptionHandler,
            StepConfiguration stepConfiguration) : this(name, receiver, transformerConfig, exceptionHandler)
        {
            _stepConfiguration = stepConfiguration;
        }

        [ExcludeFromCodeCoverage]
        internal Agent(
            string name,
            IReceiver receiver,
            Transformer transformerConfig,
            IAgentExceptionHandler exceptionHandler,
            (ConditionalStepConfig happyPath, ConditionalStepConfig unhappyPath) pipelineConfig)
        {
            _receiver = receiver;
            _transformerConfig = transformerConfig;
            _exceptionHandler = exceptionHandler;
            _conditionalPipeline = pipelineConfig;

            AgentConfig = new AgentConfig(name);
        }

        public AgentConfig AgentConfig { get; }

        /// <summary>
        /// Starts the specified agent.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        public Task Start(CancellationToken cancellation)
        {
            Logger.Debug($"Start {AgentConfig.Name}...");
            cancellation.Register(Stop);

            Task task = Task.Factory.StartNew(
                () => _receiver.StartReceiving(OnReceived, cancellation),
                TaskCreationOptions.LongRunning);

            Logger.Info($"{AgentConfig.Name} Started!");
            return task;
        }

        protected async Task<MessagingContext> OnReceived(ReceivedMessage message, CancellationToken cancellation)
        {
            MessagingContext context;

            try
            {
                ITransformer transformer = TransformerBuilder.FromTransformerConfig(_transformerConfig);
                context = await transformer.TransformAsync(message, cancellation);
            }
            catch (Exception exception)
            {
                Logger.Error($"Could not transform message: {exception.Message}");
                Logger.Trace(exception.StackTrace);
                return await _exceptionHandler.HandleTransformationException(exception, message);
            }

            return await TryExecuteSteps(context);
        }

        private async Task<MessagingContext> TryExecuteSteps(MessagingContext currentContext)
        {
            if (AgentHasNoStepsToExecute())
            {
                return currentContext;
            }

            StepResult result = StepResult.Success(currentContext);

            try
            {
                IEnumerable<IStep> steps = CreateSteps(_stepConfiguration?.NormalPipeline, _conditionalPipeline.happyPath);
                result = await ExecuteSteps(steps, currentContext);
            }
            catch (Exception exception)
            {
                return await _exceptionHandler.HandleExecutionException(exception, currentContext);
            }

            try
            {
                bool weHaveAnyUnhappyPath = _stepConfiguration?.ErrorPipeline != null || _conditionalPipeline.unhappyPath != null;
                if (result.Succeeded == false && weHaveAnyUnhappyPath && result.MessagingContext.Exception == null)
                {
                    IEnumerable<IStep> steps = CreateSteps(_stepConfiguration?.ErrorPipeline, _conditionalPipeline.unhappyPath);
                    result = await ExecuteSteps(steps, result.MessagingContext);
                }

                return result.MessagingContext;
            }
            catch (Exception exception)
            {
                return await _exceptionHandler.HandleErrorException(exception, result.MessagingContext);
            }
        }

        private bool AgentHasNoStepsToExecute()
        {
            return _conditionalPipeline.happyPath == null
                && (_stepConfiguration.NormalPipeline.Any(s => s == null) || _stepConfiguration.NormalPipeline == null);
        }

        private static IEnumerable<IStep> CreateSteps(Step[] pipeline, ConditionalStepConfig conditionalConfig)
        {
            if (pipeline != null)
            {
                return StepBuilder.FromSettings(pipeline).BuildSteps();
            }

            if (conditionalConfig != null)
            {
                return StepBuilder.FromConditionalConfig(conditionalConfig).BuildSteps();
            }

            return Enumerable.Empty<IStep>();
        }

        private static async Task<StepResult> ExecuteSteps(
            IEnumerable<IStep> steps,
            MessagingContext context)
        {
            StepResult result = StepResult.Success(context);

            var currentContext = context;

            foreach (IStep step in steps)
            {
                result = await step.ExecuteAsync(currentContext).ConfigureAwait(false);

                if (result.CanProceed == false || result.Succeeded == false || result.MessagingContext?.Exception != null)
                {
                    return result;
                }

                if (result.MessagingContext != null && currentContext != result.MessagingContext)
                {
                    currentContext = result.MessagingContext;
                }
            }

            return result;
        }

        /// <summary>
        /// Stops this agent.
        /// </summary>
        public void Stop()
        {
            Logger.Debug($"Stopping {AgentConfig.Name} ...");
            _receiver?.StopReceiving();

            Logger.Info($"{AgentConfig.Name} stopped.");
        }
    }
}
