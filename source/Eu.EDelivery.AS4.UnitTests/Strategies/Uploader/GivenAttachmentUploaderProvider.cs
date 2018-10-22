﻿using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Uploader
{
    /// <summary>
    /// Testing <see cref="GivenAttachmentUploaderProvider" />
    /// </summary>
    public class GivenAttachmentUploaderProvider
    {
        public static IEnumerable<object[]> Uploaders
        {
            get
            {
                yield return new object[] {"FILE", new FileAttachmentUploader(MimeTypeRepository.Instance)};
                yield return new object[] {"EMAIL", new EmailAttachmentUploader(MimeTypeRepository.Instance)};
                yield return new object[] {"PAYLOAD-SERVICE", new PayloadServiceAttachmentUploader()};
            }
        }

        [Theory]
        [MemberData(nameof(Uploaders))]
        public void AttachmentProviderGetsUploader_IfUploaderGetsAccepted(
            string expectedKey,
            IAttachmentUploader expectedUploader)
        {
            // Arrange
            var provider = AttachmentUploaderProvider.Instance;

            // Act
            IAttachmentUploader actualUploader = provider.Get(expectedKey);

            // Assert
            Assert.IsType(expectedUploader.GetType(), actualUploader);
        }

        [Fact]
        public void FailsToGetUploader_IfNotUploaderIsRegisteredForType()
        {
            // Arrange
            var sut = AttachmentUploaderProvider.Instance;

            // Act / Assert
            Assert.ThrowsAny<Exception>(() => sut.Get("not exsising key"));
        }
    }
}