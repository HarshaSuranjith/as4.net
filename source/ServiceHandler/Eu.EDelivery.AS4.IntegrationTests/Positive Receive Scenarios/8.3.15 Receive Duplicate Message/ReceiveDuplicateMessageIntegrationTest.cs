using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._15_Receive_Duplicate_Message
{
    /// <summary>
    /// Testing the Application with a Test Message
    /// </summary>
    public class ReceiveDuplicateMessageIntegrationTest : IntegrationTestTemplate
    {
        private const string ContentType = "multipart/related; boundary=\"=-M9awlqbs/xWAPxlvpSWrAg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

        private readonly StubSender _sender;

        public ReceiveDuplicateMessageIntegrationTest()
        {
            _sender = new StubSender();
        }

        [Fact]
        public void ThenSendingSinglePayloadSucceeds()
        {
            // Before
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);

            // Act
            Task send1 = _sender.SendMessage(Properties.Resources.duplicated_as4message, ContentType);
            send1.ContinueWith(task => CleanUpFiles(AS4FullInputPath));
            send1.Wait();

            Task send2 = _sender.SendMessage(Properties.Resources.duplicated_as4message, ContentType);
            send2.ContinueWith(task => AssertMessageIsNotDelivered());
            send2.Wait();

            // After
            StopApplication();
        }

        private static void AssertMessageIsNotDelivered()
        {
            Thread.Sleep(1000);

            var deliverDirectory = new DirectoryInfo(AS4FullInputPath);
            FileInfo[] files = deliverDirectory.GetFiles("*.xml");

            Assert.Empty(files);
        }
    }
}
