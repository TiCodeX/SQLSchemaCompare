namespace SQLCompare.Core.Entities.AccountService
{
    /// <summary>
    /// Represents response for the Azure function Account requests with a result
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    public class AzureFunctionResponse<TResult> : AzureFunctionResponse
    {
        /// <summary>
        /// Gets or sets the response result
        /// </summary>
        public TResult Result { get; set; }
    }
}