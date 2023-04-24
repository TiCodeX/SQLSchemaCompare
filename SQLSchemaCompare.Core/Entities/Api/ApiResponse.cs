namespace TiCodeX.SQLSchemaCompare.Core.Entities.Api
{
    /// <summary>
    /// Represents the response for Api requests
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the request succeeded or returned an error
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Gets or sets the response error code
        /// </summary>
        public EErrorCode ErrorCode { get; set; } = EErrorCode.Success;

        /// <summary>
        /// Gets or sets the response error message
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
