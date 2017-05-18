﻿using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing the <see cref="SendingProcessingMode" />
    /// </summary>
    public class GivenSendingProcessingModeFacts
    {
        private static readonly SendingProcessingMode DefaultEmptyPMode = new SendingProcessingMode();

        /// <summary>
        /// Testing the PMode Model for its defaults
        /// </summary>
        public class GivenDefaultSendingProcessingMode : GivenSendingProcessingModeFacts
        {
            [Fact]
            public void KeySizeIsDefault_IfNotDefined()
            {
                // Assert
                Assert.Equal(256, DefaultEmptyPMode.Security.Encryption.KeyTransport.KeySize);
            }

            [Fact]
            public void ThenEncryptionIsFalse()
            {
                // Assert
                Assert.False(DefaultEmptyPMode.Security.Encryption.IsEnabled);
            }

            [Fact]
            public void ThenErrorHandlingIsFalse()
            {
                // Assert
                Assert.False(DefaultEmptyPMode.ErrorHandling.NotifyMessageProducer);
            }

            [Fact]
            public void ThenExceptionHandlingIsFalse()
            {
                // Assert
                Assert.NotNull(DefaultEmptyPMode.ExceptionHandling);
                Assert.NotNull(DefaultEmptyPMode.ExceptionHandling.NotifyMethod);
                Assert.False(DefaultEmptyPMode.ExceptionHandling.NotifyMessageProducer);
            }

            [Fact]
            public void ThenMessagePackagingIsDefault()
            {
                // Assert
                Assert.True(DefaultEmptyPMode.MessagePackaging.UseAS4Compression);
                Assert.False(DefaultEmptyPMode.MessagePackaging.IsMultiHop);
                Assert.False(DefaultEmptyPMode.MessagePackaging.IncludePModeId);
            }

            [Fact]
            public void ThenOverrideIsFalse()
            {
                // Assert
                Assert.False(DefaultEmptyPMode.AllowOverride);
            }

            [Fact]
            public void ThenProtocolIsDefault()
            {
                // Assert
                Assert.False(DefaultEmptyPMode.PushConfiguration.Protocol.UseChunking);
                Assert.False(DefaultEmptyPMode.PushConfiguration.Protocol.UseHttpCompression);
            }

            [Fact]
            public void ThenProtocolIsNotNull()
            {
                // Assert
                Assert.NotNull(DefaultEmptyPMode.PushConfiguration);
            }

            [Fact]
            public void ThenPullConfigurationIsNotNull()
            {
                // Assert
                Assert.NotNull(DefaultEmptyPMode.PullConfiguration);
            }

            [Fact]
            public void ThenPushConfigurationIsDefault()
            {
                // Assert
                Assert.False(DefaultEmptyPMode.PushConfiguration.TlsConfiguration.IsEnabled);
                Assert.Equal(TlsVersion.Tls12, DefaultEmptyPMode.PushConfiguration.TlsConfiguration.TlsVersion);
            }

            [Fact]
            public void ThenReceiptHandlingIsFalse()
            {
                // Assert
                Assert.NotNull(DefaultEmptyPMode.ReceiptHandling);
                Assert.NotNull(DefaultEmptyPMode.ReceiptHandling.NotifyMethod);
                Assert.False(DefaultEmptyPMode.ReceiptHandling.NotifyMessageProducer);
            }

            [Fact]
            public void ThenReceiptionAwerenessIsDefault()
            {
                // Assert
                Assert.False(DefaultEmptyPMode.Reliability.ReceptionAwareness.IsEnabled);
                Assert.Equal(5, DefaultEmptyPMode.Reliability.ReceptionAwareness.RetryCount);
                Assert.Equal("00:01:00", DefaultEmptyPMode.Reliability.ReceptionAwareness.RetryInterval);
            }

            [Fact]
            public void ThenReceiptionAwerenessIsNotNull()
            {
                // Assert
                Assert.NotNull(DefaultEmptyPMode.ReceiptHandling);
                Assert.NotNull(DefaultEmptyPMode.ReceiptHandling.NotifyMethod);
            }

            [Fact]
            public void ThenReliabilityIsNotNull()
            {
                // Assert
                Assert.NotNull(DefaultEmptyPMode.Reliability);
                Assert.NotNull(DefaultEmptyPMode.Reliability.ReceptionAwareness);
            }

            [Fact]
            public void ThenSigningIsFalse()
            {
                // Assert
                Assert.NotNull(DefaultEmptyPMode.Security);
                Assert.NotNull(DefaultEmptyPMode.Security.Signing);
                Assert.False(DefaultEmptyPMode.Security.Signing.IsEnabled);
            }

            [Fact]
            public void ThenTlsConfigurationIsNotNull()
            {
                // Assert
                Assert.NotNull(DefaultEmptyPMode.PushConfiguration.TlsConfiguration);
            }
        }
    }
}