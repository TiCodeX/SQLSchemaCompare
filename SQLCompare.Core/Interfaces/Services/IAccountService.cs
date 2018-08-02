using System;
using System.Collections.Generic;
using System.Text;
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
        LoginResult CustomerInformation { get; }

        /// <summary>
        /// Create a new account
        /// </summary>
        /// <param name="email">The e-mail address that will be used as login</param>
        /// <param name="password">The password</param>
        /// <param name="startTrial">If the trial version should start immediately</param>
        /// <param name="productName">The product name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateAccountAsync(string email, string password, bool startTrial, string productName);

        /// <summary>
        /// Makes a login into the customer system
        /// </summary>
        /// <param name="email">The email address</param>
        /// <param name="password">The password</param>
        /// <param name="productName">The product name</param>
        void Login(string email, string password, string productName);

        /// <summary>
        /// Send the customer feedback
        /// </summary>
        /// <param name="evalutation">The customer evaluation value, a number between 1 and 5</param>
        /// <param name="comment">The customer comment</param>
        void SendFeedback(int evalutation, string comment);
    }
}
