namespace TiCodeX.SQLSchemaCompare.Core.Entities.Api
{
    /// <summary>
    /// Represents response for the Api requests with a result
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    public class ApiResponse<TResult> : ApiResponse
    {
        /// <summary>
        /// Gets or sets the response result
        /// </summary>
        public TResult Result { get; set; }
    }
}
