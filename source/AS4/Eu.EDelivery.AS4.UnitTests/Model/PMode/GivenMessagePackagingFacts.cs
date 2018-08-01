﻿using System;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;
using Party = Eu.EDelivery.AS4.Model.PMode.Party;

namespace Eu.EDelivery.AS4.UnitTests.Model.PMode
{
    public class GivenMessagePackagingFacts
    {
        [Fact]
        public void Then_Parties_Are_Empty_When_No_Defined()
        {
            // Arrange
            var pmode = new PartyInfo {FromParty = CreateEmptyParty(), ToParty = CreateEmptyParty()};

            // Act
            string xml = AS4XmlSerializer.ToString(pmode);

            // Assert
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            Assert.Null(doc.SelectSingleNode("/PartyInfo/FromParty"));
            Assert.Null(doc.SelectSingleNode("/PartyInfo/ToParty"));
        }

        private static Party CreateEmptyParty()
        {
            return new Party();
        }

        [Fact]
        public void Then_Parties_Are_Filled_When_Defined()
        {
            // Arrange
            var pmode = new PartyInfo {FromParty = CreateFilledParty(), ToParty = CreateFilledParty()};

            // Act
            string xml = AS4XmlSerializer.ToString(pmode);

            // Assert
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            Assert.NotNull(doc.SelectSingleNode("/PartyInfo/FromParty"));
            Assert.NotNull(doc.SelectSingleNode("/PartyInfo/ToParty"));
        }

        private static Party CreateFilledParty()
        {
            return new Party(
                role: Guid.NewGuid().ToString(),
                partyId: Guid.NewGuid().ToString());
        }
    }
}
