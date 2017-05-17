using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.UnitTests.Receivers;
using Eu.EDelivery.AS4.UnitTests.Steps;
using Eu.EDelivery.AS4.UnitTests.Transformers;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    public class SingleAgentConfig : PseudoConfig
    {
        public static Transformer TransformerConfig { get; } = new Transformer
        {
            Type = typeof(StubTransformer).AssemblyQualifiedName
        };

        public static AS4.Model.Internal.Steps ExpectedSteps { get; } = new AS4.Model.Internal.Steps
        {
            Step = new[] {new Step {Type = typeof(StubStep).AssemblyQualifiedName}}
        };

        /// <summary>
        /// Gets the settings agents.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<SettingsAgent> GetSettingsAgents()
        {
            yield return new SettingsAgent
            {
                Receiver = new Receiver {Type = typeof(StubReceiver).AssemblyQualifiedName},
                Transformer = TransformerConfig,
                Steps = ExpectedSteps
            };
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