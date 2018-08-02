using System;

namespace SQLCompare.Core.Entities.AccountService
{
    /// <summary>
    /// Represent the Login result
    /// </summary>
    public class LoginResult
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
    }
}
