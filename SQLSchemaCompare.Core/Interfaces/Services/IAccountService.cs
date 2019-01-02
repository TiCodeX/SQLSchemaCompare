using System.Threading.Tasks;
using TiCodeX.SQLSchemaCompare.Core.Entities.Api;

namespace TiCodeX.SQLSchemaCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that handles the account information
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Gets the customer information
        /// </summary>
        VerifySessionResult CustomerInformation { get; }

        /// <summary>
        /// Verify the session token and get customer information
        /// </summary>
        /// <param name="sessionToken">The session token to be verified</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task VerifySession(string sessionToken);

        /// <summary>
        /// Send the customer feedback
        /// </summary>
        /// <param name="sessionToken">The customer session token</param>
        /// <param name="rating">The customer rating value, a number between 1 and 5</param>
        /// <param name="comment">The customer comment</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendFeedback(string sessionToken, int? rating, string comment);

        /// <summary>
        /// Logout the user and reset customer information
        /// </summary>
        void Logout();
    }
}
