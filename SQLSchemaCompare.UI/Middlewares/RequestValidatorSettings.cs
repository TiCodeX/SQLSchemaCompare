namespace SQLCompare.UI.Middlewares
{
    /// <summary>
    /// Configuration options for the RequestValidator middleware
    /// </summary>
    public class RequestValidatorSettings
    {
        /// <summary>
        /// Gets or sets the specific Guid to validate the request
        /// </summary>
        public string AllowedRequestGuid { get; set; }

        /// <summary>
        /// Gets or sets the specific brower UserAgent to validate the request
        /// </summary>
        public string AllowedRequestAgent { get; set; }
    }
}
