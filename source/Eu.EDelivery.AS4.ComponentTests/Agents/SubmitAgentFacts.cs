﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{

    public class SubmitAgentFacts : ComponentTestTemplate
    {
        private readonly AS4Component _as4Msh;

        // It would be nice if this could be extracted from the configuration.
        private static readonly string HttpSubmitAgentUrl = "http://localhost:7070/msh/";

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitAgentFacts"/> class.
        /// </summary>
        public SubmitAgentFacts()
        {
            OverrideSettings("submitagent_http_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
        }

        public class GivenValidSubmitMessage : SubmitAgentFacts
        {
            [Fact]
            public async Task ThenAgentRespondsWithHttpAccepted()
            {
                using (var response = await StubSender.SendRequest(HttpSubmitAgentUrl, Encoding.UTF8.GetBytes(GetValidSubmitMessage()), "application/xml"))
                {
                    Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
                    Assert.True(String.IsNullOrWhiteSpace(response.Content.Headers.ContentType?.ToString()));
                }
            }

            [Fact]
            public async Task ThenAgentRespondsWithErrorWhenSubmitFails()
            {
                // Wait a little bit to make sure we do not delete the DB to early; otherwise it is recreated.
                await Task.Delay(1500);
                File.Delete(@".\database\messages.db");

                using (var response = await StubSender.SendRequest(HttpSubmitAgentUrl, Encoding.UTF8.GetBytes(GetValidSubmitMessage()), "application/xml"))
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    Assert.False(String.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()));
                }
            }

            private static string GetValidSubmitMessage()
            {
                return @"<?xml version=""1.0""?>
                            <SubmitMessage xmlns = ""urn:cef:edelivery:eu:as4:messages""> 
                                <Collaboration> 
                                    <AgreementRef>
                                        <PModeId>componentsubmittest-pmode</PModeId> 
                                    </AgreementRef> 
                                </Collaboration> 
                                <Payloads/>   
                            </SubmitMessage>";
            }
        }

        public class GivenInvalidSubmitMessage : SubmitAgentFacts
        {
            [Fact]
            public async Task ThenAgentRespondsWithHttpBadRequest()
            {
                using (var response = await StubSender.SendRequest(HttpSubmitAgentUrl, new byte[] { }, "application/xml"))
                {
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                }
            }

            [Fact(Skip = "This functionality has not been implemented yet. Wait for backlogitem 5983")]
            public async Task ThenDatabaseContainsInException()
            {
                var invalidSubmitMessage = GetInvalidSubmitMessage();

                using (var response = await StubSender.SendRequest(HttpSubmitAgentUrl, Encoding.UTF8.GetBytes(invalidSubmitMessage), "application/xml"))
                {
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                }

                var spy = new DatabaseSpy(_as4Msh.GetConfiguration());

                var loggedException = spy.GetInExceptions(x => String.IsNullOrWhiteSpace(x.EbmsRefToMessageId)).FirstOrDefault();

                Assert.NotNull(loggedException);
                Assert.NotNull(loggedException.MessageBody);

                Assert.Equal(invalidSubmitMessage, Encoding.UTF8.GetString(loggedException.MessageBody));
            }

            private static string GetInvalidSubmitMessage()
            {
                return @"<?xml version=""1.0""?>
                            <Submit xmlns = ""urn:cef:edelivery:eu:as4:messages""> 
                                <Collaboration> 
                                    <AgreementRef>
                                        <PModeId>componentsubmittest-pmode</PModeId> 
                                    </AgreementRef> 
                                </Collaboration> 
                                <Payload/>   
                            </Submit>";
            }
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }

    }
}
