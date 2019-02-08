﻿using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils;
using Polly;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Common
{
    [Collection(ComponentTestCollection.ComponentTestCollectionName)]
    public class ComponentTestTemplate : IDisposable
    {
        private bool _restoreSettings = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentTestTemplate"/> class.
        /// </summary>
        public ComponentTestTemplate()
        {
            ClearLogFiles();
        }

        private static void ClearLogFiles()
        {
            if (Directory.Exists(@".\logs"))
            {
                foreach (string file in Directory.GetFiles(@".\logs"))
                {
                    Policy.Handle<IOException>()
                          .Retry(3)
                          .Execute(() => File.Delete(file));
                }
            }
        }

        public string ComponentTestSettingsPath => @".\config\componenttest-settings";

        protected Settings OverrideSettings(string settingsFile)
        {
            Console.WriteLine($@"Overwrite 'settings.xml' with '{settingsFile}'");

            File.Copy(@".\config\settings.xml", @".\config\settings_original.xml", true);

            string specificSettings = $@".\config\componenttest-settings\{settingsFile}";
            File.Copy(specificSettings, @".\config\settings.xml", true);

            _restoreSettings = true;

            return AS4XmlSerializer.FromString<Settings>(File.ReadAllText(specificSettings));
        }

        protected void OverrideServiceSettings(string settingsFile)
        {
            File.Copy(@".\config\settings-service.xml", @".\config\settings_service_original.xml", true);
            File.Copy($@".\config\componenttest-settings\{settingsFile}", @".\config\settings-service.xml", true);
            _restoreSettings = true;
        }

        protected async Task TestComponentWithSettings(string settingsFile, Func<Settings, AS4Component, Task> testCase)
        {
            Settings settings = OverrideSettings(settingsFile);

            using (var as4Msh = AS4Component.Start(Environment.CurrentDirectory))
            {
                await testCase(settings, as4Msh);
            }
        }

        protected Task<TResult> PollUntilPresent<TResult>(Func<TResult> poll, TimeSpan timeout)
        {
            IObservable<TResult> polling =
                Observable.Create<TResult>(o =>
                {
                    TResult r = poll();
                    Console.WriteLine($@"Poll until present: {(r == null ? "(null)" : r.ToString())}");

                    IObservable<TResult> observable =
                        r == null
                            ? Observable.Throw<TResult>(new Exception())
                            : Observable.Return(r);
                    return observable.Subscribe(o);
                });

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            return Observable
                       .Timer(TimeSpan.FromSeconds(1))
                       .SelectMany(_ => polling)
                       .Retry()
                       .ToTask(cts.Token);
        }

        protected Task PollUntilSatisfied(Func<bool> poll, TimeSpan timeout)
        {
            var polling =
                Observable.Create<bool>(o =>
                {
                    bool result = poll();
                    Console.WriteLine($@"Poll until satisfied - result = {result}");

                    IObservable<bool> observable =
                        result == false
                            ? Observable.Throw<bool>(new Exception())
                            : Observable.Return<bool>(true);
                    return observable.Subscribe(o);
                });

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            return Observable
                .Timer(TimeSpan.FromSeconds(1))
                .SelectMany(_ => polling)
                .Retry()
                .ToTask(cts.Token);
        }

        public void Dispose()
        {
            Disposing(true);
            if (_restoreSettings && File.Exists(@".\config\settings_original.xml"))
            {
                File.Copy(@".\config\settings_original.xml", @".\config\settings.xml", true);
            }

            if (_restoreSettings && File.Exists(@".\config\settings_service_original.xml"))
            {
                File.Copy(@".\config\settings_service_original.xml", @".\config\settings-service.xml", true);
            }

            AS4Component.WriteLogFilesToConsole();
        }

        protected virtual void Disposing(bool isDisposing) { }
    }

    [CollectionDefinition(ComponentTestCollectionName)]
    public class ComponentTestCollection : ICollectionFixture<ComponentTestFixture>
    {
        public const string ComponentTestCollectionName = "ComponentTestCollection";
    }

    public class ComponentTestFixture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentTestFixture"/> class.
        /// </summary>
        public ComponentTestFixture()
        {
            FileSystemUtils.CreateOrClearDirectory(@".\config\send-pmodes");
            FileSystemUtils.CreateOrClearDirectory(@".\config\receive-pmodes");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\in");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\out");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\receipts");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\errors");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\exceptions");

            FileSystemUtils.CopyDirectory(@".\config\componenttest-settings\send-pmodes", @".\config\send-pmodes");
            FileSystemUtils.CopyDirectory(@".\config\componenttest-settings\receive-pmodes", @".\config\receive-pmodes");

            FileSystemUtils.CopyDirectory(@".\samples\pmodes\", @".\config\receive-pmodes", "*receive-pmode.xml");
        }
    }
}
