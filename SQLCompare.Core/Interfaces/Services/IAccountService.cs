using System.Threading.Tasks;
using SQLCompare.Core.Entities.AccountService;

namespace SQLCompare.Core.Interfaces.Services
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
        /// <param name="evalutation">The customer evaluation value, a number between 1 and 5</param>
        /// <param name="comment">The customer comment</param>
        void SendFeedback(int evalutation, string comment);

        /// <summary>
        /// Logout the user and reset customer information
        /// </summary>
        void Logout();
    }
}
