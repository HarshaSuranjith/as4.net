﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Fe.Settings;
using Eu.EDelivery.AS4.Model.Internal;
using NSubstitute;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class As4SettingsServiceTests
    {
        private const string SubmitAgentName = "submitAgentName";
        private const string SubmitAgentName2 = "submitAgentName2";
        private const string ReceiveAgentName = "receiveAgentName";
        private readonly Model.Internal.Settings settingsList;

        private readonly SettingsAgent submitAgent = new SettingsAgent
        {
            Name = SubmitAgentName
        };

        private As4SettingsService settingsService;
        private ISettingsSource settingsSource;

        public As4SettingsServiceTests()
        {
            settingsList = new Model.Internal.Settings
            {
                Agents = new SettingsAgents
                {
                    SubmitAgents = new List<SettingsAgent>
                    {
                        submitAgent,
                        new SettingsAgent
                        {
                            Name = SubmitAgentName2
                        }
                    }.ToArray(),
                    ReceiveAgents = new List<SettingsAgent>
                    {
                        new SettingsAgent
                        {
                            Name = ReceiveAgentName
                        }
                    }.ToArray()
                }
            };
        }

        private As4SettingsServiceTests Setup()
        {
            var mapper = Mapper.Configuration.CreateMapper();
            settingsSource = Substitute.For<ISettingsSource>();
            settingsSource.Get().Returns(settingsList);
            settingsService = new As4SettingsService(mapper, settingsSource);
            return this;
        }

        public class CreateAgent : As4SettingsServiceTests
        {
            [Fact]
            public async Task Add_Agent_When_Original_List_Is_Empty()
            {
                // Setup
                var newAgent = new SettingsAgent
                {
                    Name = "newAgent"
                };

                var test = Setup();
                test.settingsSource.Get().Returns(new Model.Internal.Settings
                {
                    Agents = new SettingsAgents()
                });

                // Act
                await test.settingsService.CreateAgent(newAgent, agents => agents.ReceiveAgents, (settings, agt) => settings.ReceiveAgents = agt);

                // Assert
                await test.settingsSource.Received().Save(Arg.Is<Model.Internal.Settings>(settings => settings.Agents.ReceiveAgents.Any(agent => agent.Name == newAgent.Name)));
            }

            [Fact]
            public async Task Creates_Agent_When_It_Doesnt_Exist_Yet()
            {
                // Setup
                var newAgentName = "newAgent";
                var newAgent = new SettingsAgent
                {
                    Name = newAgentName
                };

                var service = Setup().settingsService;

                // Act & Assert
                await service.CreateAgent(newAgent, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
                await settingsSource.Received().Save(Arg.Is<Model.Internal.Settings>(settings => settings.Agents.SubmitAgents.Any(agent => agent.Name == newAgentName)));

                await service.CreateAgent(newAgent, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
                await settingsSource.Received().Save(Arg.Is<Model.Internal.Settings>(settings => settings.Agents.ReceiveAgents.Any(agent => agent.Name == newAgentName)));
            }

            [Fact]
            public async Task Throws_Exception_When_Agent_With_Name_Already_Exists()
            {
                // Setup
                var newAgent = new SettingsAgent {Name = SubmitAgentName};

                var service = Setup().settingsService;

                // Act & Assert
                await Assert.ThrowsAsync(typeof(Exception), () => service.CreateAgent(newAgent, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents));
            }

            [Fact]
            public async Task Throws_Exception_When_Arguments_Are_Null()
            {
                // Act & Assert
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.CreateAgent(null, null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.CreateAgent(new SettingsAgent(), null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.CreateAgent(new SettingsAgent(), agents => agents.SubmitAgents, null));
            }
        }

        public class UpdateAgent : As4SettingsServiceTests
        {
            [Fact]
            public async Task Throws_Exception_When_Agent_Not_Found()
            {
                // Act & Assert
                await Assert.ThrowsAsync(typeof(Exception), () => Setup().settingsService.UpdateAgent(submitAgent, "fdsqfd", settings => settings.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents));
            }

            [Fact]
            public async Task Throws_Exception_When_Agent_With_Name_Already_Exists()
            {
                // Act
                await Assert.ThrowsAsync(typeof(Exception), () => Setup().settingsService.UpdateAgent(new SettingsAgent
                {
                    Name = SubmitAgentName
                }, SubmitAgentName2, settings => settings.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents));
            }

            [Fact]
            public async Task Throws_Exception_When_parameters_Are_Null()
            {
                // Act & Assert
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.UpdateAgent(null, null, null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.UpdateAgent(new SettingsAgent(), null, null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.UpdateAgent(new SettingsAgent(), "test", null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.UpdateAgent(new SettingsAgent(), "test", agents => agents.ReceiveAgents, null));
            }

            [Fact]
            public async Task Updates()
            {
                // Act
                await Setup().settingsService.UpdateAgent(new SettingsAgent
                {
                    Name = "NEW"
                }, submitAgent.Name, settings => settings.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);

                // Assert
                Assert.True(settingsList.Agents.SubmitAgents.Any(agent => agent.Name == "NEW"));
                await settingsSource.Received().Save(Arg.Is<Model.Internal.Settings>(x => x.Agents.SubmitAgents.Any(agt => agt.Name == "NEW")));
            }
        }

        public class DeleteAgent : As4SettingsServiceTests
        {
            [Fact]
            public async Task Deletes_Agent()
            {
                // Act & Assert
                await Setup().settingsService.DeleteAgent(SubmitAgentName, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
                await settingsSource.Received().Save(Arg.Is<Model.Internal.Settings>(x => x.Agents.SubmitAgents.All(agt => agt.Name != SubmitAgentName)));
                await Setup().settingsService.DeleteAgent(ReceiveAgentName, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
                await settingsSource.Received().Save(Arg.Is<Model.Internal.Settings>(x => x.Agents.ReceiveAgents.All(agt => agt.Name != SubmitAgentName)));

                await settingsSource.Received().Get();
            }

            [Fact]
            public async Task Throws_Exception_when_Agent_Not_Exists()
            {
                // Setup
                Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(Exception), () => settingsService.DeleteAgent("IDONTEXISTAGENT", agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents));
                await Assert.ThrowsAsync(typeof(Exception), () => settingsService.DeleteAgent("IDONTEXISTAGENT", agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents));
                await settingsSource.DidNotReceive().Save(Arg.Any<Model.Internal.Settings>());
            }

            [Fact]
            public async Task Throws_Exception_When_Parameters_Are_Null()
            {
                // Act & Assert
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.DeleteAgent(null, null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.DeleteAgent("TEST", null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.DeleteAgent("TEST", agents => agents.SubmitAgents, null));
            }
        }
    }
}