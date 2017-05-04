﻿using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Receivers.Specifications;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers.Specifications
{
    public class GivenExpressionDatastoreSpecificationFacts : GivenDatastoreFacts
    {
        [Theory]
        [InlineData("Operation = ToBeDelivered AND MEP = Push")]
        [InlineData("Operation = ToBeDelivered OR MEP = Push")]
        public void GetsExpectedInMessage_IfAndExpression(string filter)
        {
            // Arrange
            ExpressionDatastoreSpecification specification = CreateExpressionWith("InMessages", filter);

            // Act
            var expectedMessage = new InMessage {Operation = Operation.ToBeDelivered, MEP = MessageExchangePattern.Push};
            IEnumerable<Entity> actualMessages = RunExpressionFor(specification, expectedMessage);

            // Assert
            Assert.Equal(expectedMessage, actualMessages.First() as InMessage);
        }

        private IEnumerable<Entity> RunExpressionFor(IDatastoreSpecification specification, InMessage expectedMessage)
        {
            using (DatastoreContext stubDatastore = CreateDatastoreWith(expectedMessage))
            {
                return specification.GetExpression().Compile()(stubDatastore).ToList();
            }
        }

        private static ExpressionDatastoreSpecification CreateExpressionWith(string table, string filter)
        {
            var args = new DatastoreSpecificationArgs(table, filter);
            var expression = new ExpressionDatastoreSpecification();
            expression.Configure(args);

            return expression;
        }

        private DatastoreContext CreateDatastoreWith(InMessage expectedMessage)
        {
            var context = new DatastoreContext(Options);
            context.InMessages.Add(new InMessage {Operation = Operation.DeadLettered, MEP = MessageExchangePattern.Push});
            context.InMessages.Add(expectedMessage);
            context.SaveChanges();

            return context;
        }
    }
}
