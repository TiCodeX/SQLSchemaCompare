using System;
using SQLCompare.Core.Entities.AccountService;

namespace SQLCompare.Core.Entities.Exceptions
{
    /// <summary>
    /// The exception that is thrown if the response of the Account Service has error
    /// </summary>
    public class AccountServiceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountServiceException"/> class
        /// </summary>
        public AccountServiceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountServiceException"/> class
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AccountServiceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountServiceException"/> class
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public AccountServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets or sets the account service error code
        /// </summary>
        public EErrorCode ErrorCode { get; set; }
    }
}
