namespace SQLCompare.Core.Entities.AccountService
{
    /// <summary>
    /// Represents a request for the CreateAccount azure function
    /// </summary>
    public class CreateAccountRequest
    {
        /// <summary>
        /// Gets or sets the account e-mail address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the account password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a trial subscription should start immediately
        /// </summary>
        public bool StartTrial { get; set; }

        /// <summary>
        /// Gets or sets the product name
        /// </summary>
        public string ProductName { get; set; }
    }
}
