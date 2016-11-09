﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Watchers;
using NLog;

namespace Eu.EDelivery.AS4.Common
{
    /// <summary>
    /// Responsible for making sure that every child (ex. Step) is executed in the same Context
    /// </summary>
    public sealed class Config : IConfig
    {
        private static readonly IConfig Singleton = new Config();
        private readonly ILogger _logger;

        private readonly IDictionary<string, string> _configuration;
        private readonly ConcurrentDictionary<string, ConfiguredPMode> _sendingPModes;
        private readonly ConcurrentDictionary<string, ConfiguredPMode> _receivingPModes;

        private Settings _settings;
        private List<SettingsAgent> _agents;

        public static Config Instance => (Config) Singleton;
        public bool IsInitialized { get; private set; }

        internal Config()
        {
            this._logger = LogManager.GetCurrentClassLogger();

            this._configuration = new Dictionary
                <string, string>(StringComparer.CurrentCultureIgnoreCase);

            this._sendingPModes = new ConcurrentDictionary<string, ConfiguredPMode>();
            this._receivingPModes = new ConcurrentDictionary<string, ConfiguredPMode>();
        }

        /// <summary>
        /// Initialize Configuration
        /// <exception cref="AS4Exception">Thrown when Local Configuration doesn't get retrieved correctly</exception>
        /// </summary>
        public void Initialize()
        {
            try
            {
                this.IsInitialized = true;
                RetrieveLocalConfiguration();

                new PModeWatcher<SendingProcessingMode>(GetSendPModeFolder(), this._sendingPModes);
                new PModeWatcher<ReceivingProcessingMode>(GetReceivePModeFolder(), this._receivingPModes);

                LoadExternalAssemblies();
            }
            catch (Exception exception)
            {
                this._logger.Error(exception.Message);
            }
        }

        private void LoadExternalAssemblies()
        {
            DirectoryInfo externalDictionary = GetExternalDirectory();
            if (externalDictionary == null) return;

            foreach (FileInfo assemblyFile in externalDictionary.GetFiles("*.dll"))
            {
                Assembly assembly = Assembly.LoadFrom(assemblyFile.FullName);
                AppDomain.CurrentDomain.Load(assembly.GetName());
            }
        }

        private DirectoryInfo GetExternalDirectory()
        {
            DirectoryInfo directory = null;
            if(Directory.Exists(Properties.Resources.externalfolder))
                directory = new DirectoryInfo(Properties.Resources.externalfolder);

            return directory;
        }

        private string GetSendPModeFolder()
        {
            return Path.Combine(
                Properties.Resources.configurationfolder,
                Properties.Resources.sendpmodefolder);
        }

        private string GetReceivePModeFolder()
        {
            return Path.Combine(
                Properties.Resources.configurationfolder,
                Properties.Resources.receivepmodefolder);
        }

        private void RetrieveLocalConfiguration()
        {
            string path = Path.Combine(
                Properties.Resources.configurationfolder,
                Properties.Resources.settingsfilename);

            this._settings = TryDeserialize<Settings>(path);
            AssignSettingsToGlobalConfiguration();
        }

        private T TryDeserialize<T>(string path) where T : class
        {
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(fileStream) as T;
                }
            }
            catch (Exception)
            {
                this._logger.Error($"Cannot Deserialize PMode on location: {path}");
                return null;
            }
        }

        private void AssignSettingsToGlobalConfiguration()
        {
            AddFixedSettigns();
            AddCustomSettings();
            AddCustomAgents();
        }

        private void AddFixedSettigns()
        {
            this._configuration["IdFormat"] = this._settings.IdFormat;
            this._configuration["Provider"] = this._settings.Database.Provider;
            this._configuration["ConnectionString"] = this._settings.Database.ConnectionString;
            this._configuration["CertificateStore"] = this._settings.CertificateStore.StoreName;
            this._configuration["CertificateRepository"] = this._settings.CertificateStore?.Repository?.Type;
        }

        private void AddCustomSettings()
        {
            if (this._settings.CustomSettings?.Setting == null) return;
            foreach (Setting setting in this._settings.CustomSettings.Setting)
                this._configuration[setting.Key] = setting.Value;
        }

        private void AddCustomAgents()
        {
            this._agents = new List<SettingsAgent>();
            this._agents.AddRange(this._settings.Agents.ReceiveAgents);
            this._agents.AddRange(this._settings.Agents.SubtmitAgents);
            this._agents.AddRange(this._settings.Agents.SendAgents);
            this._agents.AddRange(this._settings.Agents.DeliverAgents);
            this._agents.AddRange(this._settings.Agents.NotifyAgents);
            this._agents.Add(this._settings.Agents.ReceptionAwarenessAgent);
        }

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="AS4Exception"></exception>
        /// <returns></returns>
        public SendingProcessingMode GetSendingPMode(string id)
        {
            if (id == null)
                throw new AS4Exception("Given Sending PMode key is null");

            ConfiguredPMode configuredPMode = null;
            this._sendingPModes.TryGetValue(id, out configuredPMode);

            if (configuredPMode == null)
                throw new AS4Exception("Multiple keys found for Sending Processing Mode");

            return configuredPMode.PMode as SendingProcessingMode;
        }

        /// <summary>
        /// Retrieve Setting from the Global Configurations
        /// </summary>
        /// <param name="key"> Registered Key for the Setting </param>
        /// <returns>
        /// </returns>
        public string GetSetting(string key) => this._configuration.ReadOptionalProperty(key);

        /// <summary>
        /// Get the configured settings agents
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SettingsAgent> GetSettingsAgents() => this._agents;

        /// <summary>
        /// Return all the configured <see cref="ReceivingProcessingMode"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ReceivingProcessingMode> GetReceivingPModes() 
            => this._receivingPModes.Select(p => p.Value.PMode as ReceivingProcessingMode);
    }
}