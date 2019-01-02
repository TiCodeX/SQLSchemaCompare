namespace TiCodeX.SQLSchemaCompare.UI.Models
{
    /// <summary>
    /// Represent a feedback request
    /// </summary>
    public class FeedbackRequest
    {
        /// <summary>
        /// Gets or sets the feedback rating
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// Gets or sets the comment message
        /// </summary>
        public string Comment { get; set; }
    }
}
