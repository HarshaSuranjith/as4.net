﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.IntegrationTests.Fixture;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    /// <summary>
    /// Integration Test Class to Perform common Tasks
    /// </summary>
    [Collection(HolodeckCollection.CollectionId)]
    public class IntegrationTestTemplate : IDisposable
    {
        public static readonly string AS4MessagesRootPath = Path.GetFullPath($@".\{Properties.Resources.submit_messages_path}");
        public static readonly string AS4FullOutputPath = Path.GetFullPath($@".\{Properties.Resources.submit_output_path}");
        public static readonly string AS4FullInputPath = Path.GetFullPath($@".\{Properties.Resources.submit_input_path}");

        protected static readonly string AS4IntegrationMessagesPath = Path.GetFullPath($@".\{Properties.Resources.submit_messages_path}\integrationtest-messages");
        protected static readonly string AS4ReceiptsPath = Path.GetFullPath($@".\{Properties.Resources.as4_component_receipts_path}");
        protected static readonly string AS4ErrorsPath = Path.GetFullPath($@".\{Properties.Resources.as4_component_errors_path}");
        protected static readonly string AS4ExceptionsPath = Path.GetFullPath($@".\{Properties.Resources.as4_component_exceptions_path}");

        protected readonly string HolodeckBInputPath = Properties.Resources.holodeck_B_input_path;
        protected static readonly string HolodeckMessagesPath = Path.GetFullPath(@".\messages\holodeck-messages");

        protected AS4Component AS4Component { get; } = new AS4Component();

        protected Holodeck Holodeck { get; } = new Holodeck();

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationTestTemplate"/> class.
        /// </summary>
        public IntegrationTestTemplate()
        {
            Console.WriteLine(Environment.NewLine);

            CopyDirectory(@".\config\integrationtest-settings", @".\config\");
            CopyDirectory(@".\config\integrationtest-settings\integrationtest-pmodes\send-pmodes", @".\config\send-pmodes");
            CopyDirectory(@".\config\integrationtest-settings\integrationtest-pmodes\receive-pmodes", @".\config\receive-pmodes");
            CopyDirectory(@".\messages\integrationtest-messages", @".\messages");

            CleanUpFiles(@".\database");
            CleanUpFiles(AS4FullInputPath);
            CleanUpFiles(AS4FullOutputPath);
            CleanUpFiles(AS4ReceiptsPath);
            CleanUpFiles(AS4ErrorsPath);
            CleanUpFiles(AS4ExceptionsPath);

            CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            CleanUpFiles(Properties.Resources.holodeck_A_pmodes);

            CleanUpFiles(Properties.Resources.holodeck_A_input_path);
            CleanUpFiles(Properties.Resources.holodeck_B_input_path);

            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            CleanUpFiles(Properties.Resources.holodeck_B_output_path);

            CleanUpDirectory(Path.GetFullPath(@".\database\as4messages"));
        }

        #region Fixture Setup
        private static void CopyDirectory(string sourceDirName, string destDirName)
        {
            DirectoryInfo sourceDirectory = GetSourceDirectory(sourceDirName);

            EnsureDestinationDirectory(destDirName);

            CopyFilesFromDestinationToSource(sourceDirectory, destDirName);
        }

        private static DirectoryInfo GetSourceDirectory(string sourceDirName)
        {
            var sourceDirectory = new DirectoryInfo(sourceDirName);

            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            return sourceDirectory;
        }

        private static void EnsureDestinationDirectory(string destDirName)
        {
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
        }

        private static void CopyFilesFromDestinationToSource(DirectoryInfo sourceDirectory, string destDirName)
        {
            FileInfo[] files = sourceDirectory.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, overwrite: true);
            }
        }

        protected static void ReplaceTokenInFile(string token, string value, string filePath)
        {
            string oldContents = File.ReadAllText(filePath);
            string newContents = oldContents.Replace(token, value);

            File.WriteAllText(filePath, newContents);
        }

        #endregion

        private static void CleanUpDirectory(string directoryPath)
        {
            EnsureDirectory(directoryPath);
            Try(() => Directory.Delete(directoryPath, recursive: true));
        }

        private static void Try(Action action)
        {
            if (!TryOnce(action))
            {
                Console.WriteLine(@"Retrying...");
                TryOnce(action);
            }
        }

        private static bool TryOnce(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Cleanup files in a given Directory
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="predicateFile">The predicate File.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        protected void CleanUpFiles(string directory, Func<string, bool> predicateFile = null)
        {
            EnsureDirectory(directory);

            Console.WriteLine($@"Deleting files at location: {directory}");
            
            foreach (string file in Directory.EnumerateFiles(Path.GetFullPath(directory)))
            {
                if (predicateFile == null || predicateFile(file))
                {
                    WhileTimeOutTry(retryCount: 10, retryAction: () => File.Delete(file));
                }
            }
        }

        private static void WhileTimeOutTry(int retryCount, Action retryAction)
        {
            var count = 0;

            while (count < retryCount)
            {
                try
                {
                    retryAction();
                    return;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    count++;

                    Thread.Sleep(TimeSpan.FromSeconds(3));
                }
            }
        }

        /// <summary>
        /// Poll to a given target Directory to find files
        /// </summary>
        /// <param name="directoryPath">Directory Path to poll</param>
        /// <param name="extension"></param>
        /// <param name="retryCount">Retry Count in miliseconds</param>
        protected bool PollingAt(string directoryPath, string extension = "*", int retryCount = 2500)
        {
            string location = FindAliasLocation(directoryPath);
            Console.WriteLine($@"Start polling to {location}");

            var i = 0;
            var areFilesFound = false;

            while (i < retryCount)
            {
                areFilesFound = IsMessageFound(directoryPath, extension);
                if (areFilesFound)
                {
                    break;
                }

                Thread.Sleep(2000);
                i += 100;
            }

            StopApplication();
            return areFilesFound;
        }

        private static string FindAliasLocation(string directoryPath)
        {
            if (directoryPath.Contains("holodeck"))
            {
                return "Holodeck";
            }

            if (directoryPath.Contains("messages"))
            {
                return "AS4 Component";
            }

            return directoryPath;
        }

        private bool IsMessageFound(string directoryPath, string extension)
        {
            var startDir = new DirectoryInfo(directoryPath);
            FileInfo[] files = startDir.GetFiles(extension, SearchOption.AllDirectories);
            bool areFilesPresent = files.Length > 0;

            if (!areFilesPresent)
            {
                return false;
            }

            WriteFilesToConsole(files);
            ValidatePolledFiles(files);

            return true;
        }

        private static void WriteFilesToConsole(IEnumerable<FileInfo> files)
        {
            foreach (FileInfo file in files)
            {
                Console.WriteLine($@"File found at {file.DirectoryName}: {file.Name}");
            }
        }

        private static void EnsureDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected virtual void ValidatePolledFiles(IEnumerable<FileInfo> files) { }

        /// <summary>
        /// Stop the _AS4 Component.
        /// </summary>
        protected void StopApplication()
        {
            Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with 
        /// freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            AS4Component.Dispose();

            DisposeChild();
        }

        /// <summary>
        /// Dispose custom resources in subclass implementation.
        /// </summary>
        protected virtual void DisposeChild() {}
    }
}