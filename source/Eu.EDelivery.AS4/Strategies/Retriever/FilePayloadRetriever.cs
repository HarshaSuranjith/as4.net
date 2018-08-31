using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Retriever
{
    /// <summary>
    /// File Retriever Implementation to retrieve the FileStream of a local file
    /// </summary>
    public class FilePayloadRetriever : IPayloadRetriever
    {
        public const string Key = "file:///";

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Retrieve <see cref="Stream"/> contents from a given <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public Task<Stream> RetrievePayloadAsync(string location)
        {
            return Task.FromResult(RetrievePayload(location));
        }

        private static Stream RetrievePayload(string location)
        {
            Stream payloadStream = TryRetrievePayload(location);

            Logger.Debug($"Payload is successfully retrieved at location \"{location}\"");

            return payloadStream;
        }

        private static Stream TryRetrievePayload(string location)
        {
            return RetrievePayloadAtlocation(location);
        }

        private static Stream RetrievePayloadAtlocation(string location)
        {
            string relativePath = location.Replace("file:///", string.Empty);
            string absolutePath = Path.GetFullPath(Path.Combine(Config.ApplicationPath, relativePath));

            var payload = new FileInfo(absolutePath);
            var supportedPayloadDir = new DirectoryInfo(Path.Combine(Config.ApplicationPath, "messages", "attachments"));

            // TODO: if we only allow this folder, maybe we should also allow just the filename in the SubmitMessage
            if (payload.Directory != supportedPayloadDir)
            {
                throw new NotSupportedException(
                    "Only files from the 'messages/attachments/' folder are allowed to be retrieved");
            }

            var uri = new Uri(absolutePath);            
            
            return new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}