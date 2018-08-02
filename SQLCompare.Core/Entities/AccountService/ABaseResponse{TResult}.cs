namespace SQLCompare.Core.Entities.AccountService
{
    /// <summary>
    /// Represents the base response for AccountService requests with a result
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    public abstract class ABaseResponse<TResult>
    {
        /// <summary>
        /// Gets or sets the response result
        /// </summary>
        public TResult Result { get; set; }
    }
}