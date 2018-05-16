﻿namespace Eu.EDelivery.AS4.Strategies.Sender
{
    public class DeliverResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverResult"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="needsAnotherRetry">if set to <c>true</c> [needs another retry].</param>
        public DeliverResult(DeliveryStatus status, bool needsAnotherRetry)
        {
            Status = status;
            NeedsAnotherRetry = needsAnotherRetry;
        }

        /// <summary>
        /// Gets the status indicating whether the <see cref="DeliverResult"/> is successful or not.
        /// </summary>
        /// <value>The status.</value>
        public DeliveryStatus Status { get; }

        /// <summary>
        /// Gets a value indicating whether [another retry is needed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [another retry is needed]; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsAnotherRetry { get; }

        /// <summary>
        /// Creates a successful representation of the <see cref="DeliverResult"/>.
        /// </summary>
        /// <returns></returns>
        public static DeliverResult Success { get; } = new DeliverResult(DeliveryStatus.Successful, needsAnotherRetry: false);

        /// <summary>
        /// Reduces the two specified <see cref="DeliverResult"/>'s.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public static DeliverResult Reduce(DeliverResult x, DeliverResult y)
        {
            return new DeliverResult(
                x.Status == DeliveryStatus.Successful 
                && y.Status == DeliveryStatus.Successful 
                    ? DeliveryStatus.Successful
                    : DeliveryStatus.Failure,
                x.NeedsAnotherRetry || y.NeedsAnotherRetry);
        }

        /// <summary>
        /// Creates as failure representation of the <see cref="DeliverResult"/>.
        /// </summary>
        /// <param name="anotherRetryIsNeeded">if set to <c>true</c> [another retry is needed].</param>
        /// <returns></returns>
        public static DeliverResult Failure(bool anotherRetryIsNeeded)
        {
            return new DeliverResult(DeliveryStatus.Failure, anotherRetryIsNeeded);
        }
    }

    public enum DeliveryStatus
    {
        Successful,
        Failure
    }
}