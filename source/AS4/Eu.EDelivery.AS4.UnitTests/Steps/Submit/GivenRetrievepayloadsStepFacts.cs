﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    /// <summary>
    /// Testing the <see cref="RetrievePayloadsStep" />
    /// </summary>
    public class GivenRetrievePayloadsStepFacts : IDisposable
    {
        private IPayloadRetrieverProvider _provider;
        private MemoryStream _memoryStream;
        private RetrievePayloadsStep _step;

        public GivenRetrievePayloadsStepFacts()
        {
            SetupProvider();
            this._step = new RetrievePayloadsStep(this._provider);
        }

        private void SetupProvider()
        {
            this._provider = new PayloadRetrieverProvider();
            this._provider.Accept((Payload p) => true, GetMockedPayloadStrategy().Object);
        }

        private Mock<IPayloadRetriever> GetMockedPayloadStrategy()
        {
            this._memoryStream = new MemoryStream();

            var mockedPayloadStrategy = new Mock<IPayloadRetriever>();
            mockedPayloadStrategy
                .Setup(s => s.RetrievePayload(It.IsAny<string>()))
                .Returns(this._memoryStream);
            return mockedPayloadStrategy;
        }

        private SubmitMessage GetStubSubmitMessage()
        {
            return new SubmitMessage {Payloads = new[] {new Payload(location: "file:")}};
        }

        /// <summary>
        /// Testing if the Step succeeds
        /// with valid arguments for the "Execute" Method
        /// </summary>
        public class GivenValidArgumentsToExecute : GivenRetrievePayloadsStepFacts
        {
            [Fact]
            public async Task ThenExecuteReturnsZeroAttachmentsAsync()
            {
                // Arrange
                base._step = new RetrievePayloadsStep(base._provider);
                var message = new InternalMessage(new AS4Message()) {SubmitMessage = new SubmitMessage()};
                // Act
                StepResult result = await base._step.ExecuteAsync(message, CancellationToken.None);
                // Assert
                Assert.NotNull(result);
                Assert.Equal(0, result.InternalMessage.AS4Message.Attachments.Count);
            }

            [Fact]
            public async Task ThenExeucteReturnsAttachmentsFromPayloadsAsync()
            {
                // Arrange
                var step = new RetrievePayloadsStep(base._provider);
                InternalMessage message = GetInternalMessage();
                // Act
                StepResult result = await step.ExecuteAsync(message, CancellationToken.None);
                // Assert
                Assert.NotNull(result);
                Attachment attachment = GetAttachment(result);
                Assert.Equal(base._memoryStream, attachment.Content);
            }

            private InternalMessage GetInternalMessage()
            {
                return new InternalMessage
                {
                    SubmitMessage = base.GetStubSubmitMessage(),
                    AS4Message = new AS4Message()
                };
            }

            private Attachment GetAttachment(StepResult result)
            {
                Attachment attachment = null;
                IEnumerator<Attachment> enumerator = result.InternalMessage
                    .AS4Message.Attachments.GetEnumerator();

                while (enumerator.MoveNext())
                    attachment = enumerator.Current;

                Assert.NotNull(attachment);
                return attachment;
            }
        }

        public void Dispose()
        {
            this._memoryStream.Dispose();
        }
    }
}