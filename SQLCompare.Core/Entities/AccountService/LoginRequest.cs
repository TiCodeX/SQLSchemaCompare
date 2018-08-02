namespace SQLCompare.Core.Entities.AccountService
{
    /// <summary>
    /// Represents a request for the Login azure function
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Gets or sets the login e-mail address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the login password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the product name
        /// </summary>
        public string ProductName { get; set; }
    }
}
