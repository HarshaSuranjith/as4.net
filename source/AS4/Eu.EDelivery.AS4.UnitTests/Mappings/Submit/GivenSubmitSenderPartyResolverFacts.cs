﻿using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;
using CommonParty = Eu.EDelivery.AS4.Model.Common.Party;
using CoreParty = Eu.EDelivery.AS4.Model.Core.Party;
using PartyInfo = Eu.EDelivery.AS4.Model.PMode.PartyInfo;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitSenderPartyResolver" />
    /// </summary>
    public class GivenSubmitSenderPartyResolverFacts
    {
        public class GivenValidArguments : GivenSubmitSenderPartyResolverFacts
        {
            [Fact]
            public void ThenResolverGetsDefaultParty()
            {
                // Arrange
                var submitMessage = new SubmitMessage {PMode = new SendingProcessingMode()};
                var resolver = SubmitSenderPartyResolver.Default;

                // Act
                CoreParty party = resolver.Resolve(submitMessage);

                // Assert
                Assert.Equal(Constants.Namespaces.EbmsDefaultFrom, party.PartyIds.First().Id);
                Assert.Equal(Constants.Namespaces.EbmsDefaultRole, party.Role);
            }

            [Fact]
            public void ThenResolverGetsPartyFromPMode()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    PMode =
                        new SendingProcessingMode
                        {
                            MessagePackaging = {PartyInfo = new PartyInfo {FromParty = CreatePopulatedCoreParty()}}
                        }
                };
                var resolver = SubmitSenderPartyResolver.Default;

                // Act
                CoreParty party = resolver.Resolve(submitMessage);

                // Assert
                Assert.Equal(submitMessage.PMode.MessagePackaging.PartyInfo.FromParty, party);
            }

            [Fact]
            public void ThenResolverGetsPartyFromSubmitMessage()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    PartyInfo = {FromParty = CreatePopulatedCommonParty()},
                    PMode = new SendingProcessingMode {AllowOverride = true}
                };
                var resolver = SubmitSenderPartyResolver.Default;

                // Act
                CoreParty party = resolver.Resolve(submitMessage);

                // Assert
                CommonParty fromParty = submitMessage.PartyInfo.FromParty;
                Assert.Equal(fromParty.Role, party.Role);
                Assert.Equal(fromParty.PartyIds.First().Id, party.PartyIds.First().Id);
                Assert.Equal(fromParty.PartyIds.First().Type, party.PartyIds.First().Type);
            }
        }

        public class GivenInvalidArguments : GivenSubmitSenderPartyResolverFacts
        {
            [Fact]
            public void ThenResolverFailsWhenOverrideIsNotAllowed()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    PartyInfo = {FromParty = CreatePopulatedCommonParty()},
                    PMode =
                        new SendingProcessingMode
                        {
                            AllowOverride = false,
                            MessagePackaging = {PartyInfo = new PartyInfo {FromParty = CreatePopulatedCoreParty()}}
                        }
                };

                var resolver = SubmitSenderPartyResolver.Default;

                // Act / Assert
                Assert.ThrowsAny<Exception>(() => resolver.Resolve(submitMessage));
            }
        }

        protected CommonParty CreatePopulatedCommonParty()
        {
            return new CommonParty {Role = "submit-role", PartyIds = new[] {new PartyId("submit-id", "submit-type")}};
        }

        protected CoreParty CreatePopulatedCoreParty()
        {
            return new CoreParty
            {
                Role = "pmode-role",
                PartyIds = new List<AS4.Model.Core.PartyId> {new AS4.Model.Core.PartyId("pmode-id")}
            };
        }
    }
}