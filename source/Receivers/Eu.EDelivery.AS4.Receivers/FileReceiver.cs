﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security;
using NLog;
using Function =
    System.Func<Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.MessagingContext>>;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// <see cref="IReceiver" /> Implementation to receive Files
    /// </summary>
    [Info("File receiver")]
    public class FileReceiver : PollingTemplate<FileInfo, ReceivedMessage>, IReceiver
    {        
        private readonly SynchronizedCollection<FileInfo> _pendingFiles = new SynchronizedCollection<FileInfo>();
        private readonly IMimeTypeRepository _repository;

        private IDictionary<string, string> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileReceiver" /> class
        /// </summary>
        public FileReceiver()
        {
            _repository = new MimeTypeRepository();
            Logger = LogManager.GetCurrentClassLogger();
        }

        [Info("File path")]
        [Description("Path to the folder to poll for new files")]
        private string FilePath => _properties.ReadMandatoryProperty(SettingKeys.FilePath);

        [Info("File mask")]
        private string FileMask => _properties.ReadOptionalProperty(SettingKeys.FileMask, "*.*");

        [Info("Username")]
        private string Username => _properties.ReadOptionalProperty(SettingKeys.UserName);

        [Info("Password")]
        private string Password => _properties.ReadOptionalProperty(SettingKeys.Password);

        private int _batchSize;

        protected override ILogger Logger { get; }

        [Info("Polling interval", "", "int")]
        protected override TimeSpan PollingInterval => FromProperties();

        #region Configuration

        private static class SettingKeys
        {
            public const string FilePath = "FilePath";
            public const string FileMask = "FileMask";
            public const string UserName = "Username";
            public const string Password = "Password";
            public const string BatchSize = "BatchSize";
        }

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        public void Configure(IEnumerable<Setting> settings)
        {
            _properties = settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

            var configuredBatchSize = _properties.ReadOptionalProperty(SettingKeys.BatchSize, "20");

            if (Int32.TryParse(configuredBatchSize, out _batchSize) == false)
            {
                _batchSize = 20;
            }
        }

        #endregion

        /// <summary>
        /// Start Receiving on the given File LocationParameter
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(Function messageCallback, CancellationToken cancellationToken)
        {
            Logger.Debug($"Start receiving on '{Path.GetFullPath(FilePath)}'...");
            StartPolling(messageCallback, cancellationToken);
        }

        public void StopReceiving()
        {
            Logger.Debug($"Stop receiving on '{Path.GetFullPath(FilePath)}'...");
        }

        private async Task GetMessageFromFile(FileInfo fileInfo, Function messageCallback, CancellationToken token)
        {
            if (!fileInfo.Exists)
            {
                return;
            }

            Logger.Info($"Getting Message from file '{fileInfo.Name}'");

            await OpenStreamFromMessage(fileInfo, messageCallback, token);

            _pendingFiles.Remove(fileInfo);
        }

        private async Task OpenStreamFromMessage(FileInfo fileInfo, Function messageCallback, CancellationToken token)
        {
            try
            {
                string contentType = _repository.GetMimeTypeFromExtension(fileInfo.Extension);

                var result = MoveFile(fileInfo, "processing");

                if (result.success)
                {
                    MessagingContext messagingContext = null;

                    try
                    {
                        using (Stream fileStream = new FileStream(result.filename, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.Seek(0, SeekOrigin.Begin);
                            var receivedMessage = new ReceivedMessage(fileStream, contentType);
                            messagingContext = await messageCallback(receivedMessage, token).ConfigureAwait(false);
                        }

                        await NotifyReceivedFile(fileInfo, messagingContext).ConfigureAwait(false);
                    }
                    finally
                    {
                        messagingContext?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occured while processing {fileInfo.Name}");
                Logger.Trace(ex.Message);
            }
        }

        private async Task NotifyReceivedFile(FileInfo fileInfo, MessagingContext messagingContext)
        {
            if (messagingContext.AS4Exception != null)
            {
                await HandleException(fileInfo, messagingContext.AS4Exception);
            }
            else
            {
                MoveFile(fileInfo, "accepted");
            }
        }

        private async Task HandleException(FileInfo fileInfo, AS4Exception as4Exception)
        {
            MoveFile(fileInfo, "exception");
            await CreateExceptionFile(fileInfo, as4Exception);
        }

        private async Task CreateExceptionFile(FileSystemInfo fileInfo, AS4Exception as4Exception)
        {
            string fileName = fileInfo.FullName + ".details";
            Logger.Info($"Exception Details are stored at: {fileName}");

            using (var streamWriter = new StreamWriter(fileName))
            {
                await streamWriter.WriteLineAsync(as4Exception.ToString()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Move file to another place on the File System
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="extension"></param>
        private (bool success, string filename) MoveFile(FileInfo fileInfo, string extension)
        {
            extension = extension.TrimStart('.');

            Logger.Debug($"Renaming file '{fileInfo.Name}'...");
            string destFileName =
                $"{fileInfo.Directory?.FullName}\\{Path.GetFileNameWithoutExtension(fileInfo.FullName)}.{extension}";

            try
            {
                destFileName = EnsureFilenameIsUnique(destFileName);

                int attempts = 0;

                do
                {
                    try
                    {
                        fileInfo.MoveTo(destFileName);
                        attempts = 5;
                    }
                    catch (IOException)
                    {
                        // When the file is in use, an IO exception will be thrown.
                        // If that is the case, wait a little and retry.                       
                        if (attempts == 5)
                        {
                            throw;
                        }
                        attempts++;
                        Thread.Sleep(500);
                    }
                } while (attempts < 5);

                Logger.Info($"File renamed to: '{fileInfo.Name}'!");

                return (success: true, filename: destFileName);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to MoveFile {fileInfo.FullName} to {destFileName}");
                Logger.Error(ex.Message);
                Logger.Trace(ex.StackTrace);
                return (success: false, filename: string.Empty);
            }
        }

        private static string EnsureFilenameIsUnique(string filename)
        {
            while (File.Exists(filename))
            {
                const string copyExtension = " - Copy";

                string name = Path.GetFileName(filename) + copyExtension + Path.GetExtension(filename);
                string copyFilename = Path.Combine(Path.GetDirectoryName(filename) ?? string.Empty, name);

                filename = copyFilename;
            }

            return filename;
        }

        private TimeSpan FromProperties()
        {
            string pollingInterval = _properties.ReadMandatoryProperty("PollingInterval");
            double miliseconds = Convert.ToDouble(pollingInterval);

            return TimeSpan.FromMilliseconds(miliseconds);
        }

        private void WithImpersonation(Action action)
        {
            object impersonationContext = null;

            try
            {
                impersonationContext = ImpersonateWhenRequired();
                action();
            }
            finally
            {
                if (impersonationContext != null) Impersonation.UndoImpersonation(impersonationContext);
            }
        }

        private object ImpersonateWhenRequired()
        {
            if (string.IsNullOrEmpty(Username)) return null;

            Logger.Trace($"Impersonating as user {Username}");
            return Impersonation.Impersonate(Username, Password);
        }

        /// <summary>
        /// Declaration to where the Message are and can be polled
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override IEnumerable<FileInfo> GetMessagesToPoll(CancellationToken cancellationToken)
        {
            var directoryInfo = new DirectoryInfo(FilePath);
            var resultedFiles = new List<FileInfo>();

            WithImpersonation(
                delegate
                {
                    FileInfo[] directoryFiles = directoryInfo.GetFiles(FileMask).Take(_batchSize).ToArray();

                    foreach (FileInfo file in directoryFiles)
                    {
                        try
                        {
                            var result = MoveFile(file, "pending");

                            if (result.success)
                            {
                                var pendingFile = new FileInfo(result.filename);

                                Logger.Trace(
                                    $"Locked file {file.Name} to be processed and renamed it to {pendingFile.Name}");

                                _pendingFiles.Add(pendingFile);
                               
                                resultedFiles.Add(pendingFile);
                            }
                        }
                        catch (IOException ex)
                        {
                            Logger.Info($"FileReceiver on {FilePath}: {file.Name} skipped since it is in use.");
                            Logger.Trace(ex.Message);
                        }
                    }
                });

            return resultedFiles;
        }

        protected override void ReleasePendingItems()
        {
            // Rename the 'pending' files to their original filename.
            string extension = Path.GetExtension(FileMask);

            lock (_pendingFiles.SyncRoot)
            {
                for (int i = _pendingFiles.Count - 1; i >= 0; i--)
                {
                    var pendingFile = _pendingFiles[i];

                    if (File.Exists(pendingFile.FullName))
                    {
                        MoveFile(pendingFile, extension);
                    }
                    _pendingFiles.Remove(pendingFile);
                }
            }
        }

        /// <summary>
        /// Declaration to the action that has to executed when a Message is received
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="messageCallback">Message Callback after the Message is received</param>
        /// <param name="token"></param>
        protected override void MessageReceived(FileInfo entity, Function messageCallback, CancellationToken token)
        {
            Logger.Info($"Received Message from Filesystem: {entity.Name}");
            WithImpersonation(async () => await GetMessageFromFile(entity, messageCallback, token));
        }

        /// <summary>
        /// Describe what to do in case of an Exception
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="exception"></param>
        protected override void HandleMessageException(FileInfo fileInfo, Exception exception)
        {
            Logger.Error(exception.Message);
            MoveFile(fileInfo, "exception");
        }
    }
}