﻿using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Minder send <see cref="AS4Message" /> to the corresponding Receiving MSH,
    /// with no exception handling, just catching
    /// </summary>
    public class MinderSendAS4MessageStep : IStep
    {
        private readonly ILogger _logger;
        private readonly ISerializerProvider _provider;
        private readonly ICertificateRepository _repository;

        private AS4Message _as4Message;
        private InternalMessage _internalMessage;
        private StepResult _stepResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep"/> class
        /// </summary>
        public MinderSendAS4MessageStep()
        {
            this._provider = Registry.Instance.SerializerProvider;
            this._repository = Registry.Instance.CertificateRepository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep"/> class. 
        /// Create a Send AS4Message Step 
        /// with a given Serializer Provider
        /// </summary>
        /// <param name="provider">
        /// </param>
        public MinderSendAS4MessageStep(ISerializerProvider provider)
        {
            this._provider = provider;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Send the <see cref="AS4Message" />
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            InitializeFields(internalMessage);
            await TrySendAS4MessageAsync(internalMessage, cancellationToken);

            return this._stepResult;
        }

        private void InitializeFields(InternalMessage message)
        {
            this._as4Message = message.AS4Message;
            this._stepResult = StepResult.Success(message);
            this._internalMessage = message;
        }

        private async Task TrySendAS4MessageAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            try
            {
                HttpWebRequest request = CreateWebRequest();
                await TryHandleHttpRequestAsync(request, cancellationToken);
                await TryHandleHttpResponseAsync(request, cancellationToken);
            }
            catch (Exception exception)
            {
                // [CONFORMANCE TESTING] Don't rethrow exception to not have an endless loop of retrying
                // Reason: Minder Interceptor doesn't always return a valid AS4 Message
                ThrowFailedSendAS4Exception(internalMessage, exception);
            }
        }

        private HttpWebRequest CreateWebRequest()
        {
            string protocolUrl = this._as4Message.SendingPMode.PushConfiguration.Protocol.Url;
            var request = WebRequest.Create(protocolUrl) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = this._as4Message.ContentType;
            request.KeepAlive = false;
            request.Connection = "Open";
            request.ProtocolVersion = HttpVersion.Version11;

            AssignClientCertificate(request);
            ServicePointManager.Expect100Continue = false;

            return request;
        }

        private void AssignClientCertificate(HttpWebRequest request)
        {
            TlsConfiguration configuration = this._as4Message.SendingPMode.PushConfiguration.TlsConfiguration;
            if (!configuration.IsEnabled || configuration.ClientCertificateReference == null) return;

            ClientCertificateReference certReference = configuration.ClientCertificateReference;
            X509Certificate2 certificate = this._repository
                .GetCertificate(certReference.ClientCertificateFindType, certReference.ClientCertificateFindValue);

            request.ClientCertificates.Add(certificate);
        }

        private async Task TryHandleHttpRequestAsync(WebRequest request, CancellationToken cancellationToken)
        {
            try
            {
                string url = this._as4Message.SendingPMode.PushConfiguration.Protocol.Url;
                this._logger.Info($"Send AS4 Message to: {url}");
                await HandleHttpRequestAsync(request, cancellationToken);
            }
            catch (WebException exception)
            {
                throw ThrowFailedSendAS4Exception(this._internalMessage, exception);
            }
        }

        private async Task HandleHttpRequestAsync(WebRequest request, CancellationToken cancellationToken)
        {
            ISerializer serializer = this._provider.Get(this._as4Message.ContentType);

            this._as4Message.SecurityHeader = new SecurityHeader();
            using (Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                serializer.Serialize(this._as4Message, requestStream, cancellationToken);
        }

        private async Task TryHandleHttpResponseAsync(WebRequest request, CancellationToken cancellationToken)
        {
            try
            {
                string url = this._as4Message.SendingPMode.PushConfiguration.Protocol.Url;
                this._logger.Debug($"AS4 Message receivced from: {url}");
                await HandleHttpResponseAsync(request, cancellationToken);
            }
            catch (WebException exception)
            {
                if (exception.Response != null &&
                    ContentTypeSupporter.IsContentTypeSupported(exception.Response.ContentType))
                    await PrepareStepResult(exception.Response as HttpWebResponse, cancellationToken);

                else throw ThrowFailedSendAS4Exception(this._internalMessage, exception);
            }
        }

        private async Task HandleHttpResponseAsync(WebRequest request, CancellationToken cancellationToken)
        {
            using (WebResponse responseStream = await request.GetResponseAsync().ConfigureAwait(false))
                await PrepareStepResult(responseStream as HttpWebResponse, cancellationToken);
        }

        private async Task PrepareStepResult(HttpWebResponse webResponse, CancellationToken cancellationToken)
        {
            this._stepResult.InternalMessage.AS4Message = this._as4Message;
            if (webResponse == null || webResponse.StatusCode == HttpStatusCode.Accepted)
            {
                this._stepResult.InternalMessage.AS4Message.SignalMessages.Clear();
                this._logger.Info("Empty Soap Body is returned, no further action is needed");
                return;
            }

            await DeserializeHttpResponse(webResponse, cancellationToken);
            AddExtraInfoToReceivedAS4Message();
        }

        private async Task DeserializeHttpResponse(HttpWebResponse webResponse, CancellationToken cancellationToken)
        {
            string contentType = webResponse.ContentType;
            Stream responseStream = webResponse.GetResponseStream();

            ISerializer serializer = this._provider.Get(contentType: contentType);
            this._stepResult.InternalMessage.AS4Message = await serializer
                .DeserializeAsync(responseStream, contentType, cancellationToken);
        }

        private void AddExtraInfoToReceivedAS4Message()
        {
            this._stepResult.InternalMessage.AS4Message.SendingPMode = this._as4Message.SendingPMode;
            this._stepResult.InternalMessage.AS4Message.UserMessages.Add(this._as4Message.PrimaryUserMessage);
        }

        private AS4Exception ThrowFailedSendAS4Exception(InternalMessage internalMessage, Exception exception)
        {
            string protocolUrl = this._as4Message.SendingPMode.PushConfiguration.Protocol.Url;
            string description = $"Failed to Send AS4 Message to Url: {protocolUrl}.";
            this._logger.Error(description);

            return new AS4ExceptionBuilder()
                .WithDescription(description)
                .WithErrorCode(ErrorCode.Ebms0005)
                .WithExceptionType(ExceptionType.ConnectionFailure)
                .WithMessageIds(internalMessage.AS4Message.MessageIds)
                .WithSendingPMode(internalMessage.AS4Message.SendingPMode)
                .WithInnerException(exception)
                .Build();
        }
    }
}