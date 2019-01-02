namespace TiCodeX.SQLSchemaCompare.Core.Entities.Api
{
    /// <summary>
    /// Represent the send feedback request
    /// </summary>
    public class SendFeedbackRequest
    {
        /// <summary>
        /// Gets or sets the session token
        /// </summary>
        public string SessionToken { get; set; }

        /// <summary>
        /// Gets or sets the customer rating
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// Gets or sets the customer comment
        /// </summary>
        public string Comment { get; set; }
    }
}
