﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Models;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    public class As4SettingsService : IAs4SettingsService
    {
        private readonly IMapper mapper;
        private readonly ISettingsSource settingsSource;

        public As4SettingsService(IMapper mapper, ISettingsSource settingsSource)
        {
            this.mapper = mapper;
            this.settingsSource = settingsSource;
        }

        public async Task SaveBaseSettings(BaseSettings settings)
        {
            var file = await GetSettings();
            mapper.Map(settings, file);
            await settingsSource.Save(file);
        }

        public async Task SaveCustomSettings(CustomSettings settings)
        {
            var file = await GetSettings();
            file.CustomSettings = settings;
            await settingsSource.Save(file);
        }

        public async Task SaveDatabaseSettings(SettingsDatabase settings)
        {
            var file = await GetSettings();
            mapper.Map(settings, file.Database);
            await settingsSource.Save(file);
        }

        public async Task CreateAgent(SettingsAgent settingsAgent, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents)
        {
            if (settingsAgent == null) throw new ArgumentNullException(nameof(SettingsAgent), $"Parameter {nameof(SettingsAgent)} cannot be null");
            if (getAgents == null) throw new ArgumentNullException(nameof(getAgents), $"Parameter {nameof(getAgents)} cannot be null");
            if (setAgents == null) throw new ArgumentNullException(nameof(setAgents), $"Paramter {nameof(setAgents)} cannot be null");

            var file = await GetSettings();
            var agents = getAgents(file.Agents).ToList();
            var existing = agents.FirstOrDefault(agent => agent.Name == settingsAgent.Name);
            if (existing != null)
                throw new Exception($"Agent with name {settingsAgent.Name} already exists");

            agents.Add(settingsAgent);
            setAgents(file.Agents, agents.ToArray());

            await settingsSource.Save(file);
        }

        public async Task UpdateAgent(SettingsAgent settingsAgent, string originalAgentName, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents)
        {
            if (settingsAgent == null) throw new ArgumentNullException(nameof(settingsAgent), $"Parameter {nameof(settingsAgent)} cannot be null");
            if (originalAgentName == null) throw new ArgumentNullException(nameof(originalAgentName), $"Parameter {nameof(originalAgentName)} cannot be null");
            if (getAgents == null) throw new ArgumentNullException(nameof(getAgents), $"Parameter {nameof(getAgents)} cannot be null");
            if (setAgents == null) throw new ArgumentNullException(nameof(setAgents), $"Parameter {nameof(setAgents)} cannot be null");

            var file = await GetSettings();
            var agents = getAgents(file.Agents);
            // If a rename of an agent is requested then validate that no other agent with the new name exists yet
            if (originalAgentName != settingsAgent.Name && agents.Any(agt => agt.Name == settingsAgent.Name))
            {
                throw new Exception($"An agent with name {settingsAgent.Name} already exists");
            }

            var agent = agents.FirstOrDefault(agt => agt.Name == originalAgentName);
            if (agent == null) throw new Exception($"{originalAgentName} agent doesn't exist");

            mapper.Map(settingsAgent, agent);
            await settingsSource.Save(file);
        }

        public async Task DeleteAgent(string name, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "Parameter name cannot be empty");
            if (getAgents == null) throw new ArgumentNullException(nameof(getAgents), $"{nameof(getAgents)} cannot be null");
            if (setAgents == null) throw new ArgumentNullException(nameof(setAgents), $"{nameof(setAgents)} cannot be null");

            var file = await GetSettings();
            var agents = getAgents(file.Agents);

            var agent = agents.FirstOrDefault(agt => agt.Name == name);
            if (agent == null) throw new Exception($"Submit agent {name} could not be found");
            var newList = agents.ToList();
            newList.Remove(agent);
            setAgents(file.Agents, newList.ToArray());

            await settingsSource.Save(file);
        }

        public async Task<AS4Model.Settings> GetSettings()
        {
            return await settingsSource.Get();
        }

        public Task GetByInterface<TInterface>()
        {
            return Task.FromResult(0);
        }
    }
}