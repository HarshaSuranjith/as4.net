using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.UnitTests.Receivers;
using Eu.EDelivery.AS4.UnitTests.Steps;
using Eu.EDelivery.AS4.UnitTests.Transformers;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    public class SingleAgentConfig : PseudoConfig
    {
        private static Transformer TransformerConfig { get; } = new Transformer
        {
            Type = typeof(DummyTransformer).AssemblyQualifiedName
        };

        private static Step[] ExpectedStep { get; } = {new Step {Type = typeof(DummyStep).AssemblyQualifiedName}};

        /// <summary>
        /// Gets the settings agents.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<AgentSettings> GetSettingsAgents()
        {
            yield return
                new AgentSettings
                {
                    Receiver = new Receiver {Type = typeof(StubReceiver).AssemblyQualifiedName},
                    Transformer = TransformerConfig,
                    StepConfiguration = new StepConfiguration
                    {
                        NormalPipeline = ExpectedStep,
                        ErrorPipeline = ExpectedStep
                    }
                };
        }

        /// <summary>
        /// Gets the agent settings.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<AgentConfig> GetAgentsConfiguration()
        {
            return GetSettingsAgents().Select(settings => new AgentConfig(null) {Settings = settings});
        }

        /// <summary>
        /// Gets the configuration of the Minder Test-Agents that are enabled.
        /// </summary>
        /// <returns></returns>
        /// <remarks>For every SettingsMinderAgent that is returned, a special Minder-Agent will be instantiated.</remarks>
        public override IEnumerable<SettingsMinderAgent> GetEnabledMinderTestAgents()
        {
            return Enumerable.Empty<SettingsMinderAgent>();
        }
    }

    public class SingleAgentBaseConfig : SingleAgentConfig
    {
        /// <summary>
        /// Gets the agent settings.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<AgentConfig> GetAgentsConfiguration()
        {
            return
                GetSettingsAgents()
                    .Select(settings => new AgentConfig(null) {Settings = settings, Type = AgentType.Submit});
        }

        /// <summary>
        /// Gets the configuration of the Minder Test-Agents that are enabled.
        /// </summary>
        /// <returns></returns>
        /// <remarks>For every SettingsMinderAgent that is returned, a special Minder-Agent will be instantiated.</remarks>
        public override IEnumerable<SettingsMinderAgent> GetEnabledMinderTestAgents()
        {
            return Enumerable.Empty<SettingsMinderAgent>();
        }
    }
}