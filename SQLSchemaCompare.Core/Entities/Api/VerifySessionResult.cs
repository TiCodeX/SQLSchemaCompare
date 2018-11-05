using System;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Api
{
    /// <summary>
    /// Represent the VerifySession result
    /// </summary>
    public class VerifySessionResult
    {
        /// <summary>
        /// Gets or sets the login session id
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product subscription is in trial
        /// </summary>
        public bool? IsTrial { get; set; }

        /// <summary>
        /// Gets or sets the subscription plan
        /// </summary>
        public string SubscriptionPlan { get; set; }

        /// <summary>
        /// Gets or sets the date when the subscriptions expires
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the account email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the account id
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the latest product version number
        /// </summary>
        public string LatestProductVersion { get; set; }
    }
}
