namespace TiCodeX.SQLSchemaCompare.Core.Entities.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Api;

    /// <summary>
    /// The exception that is thrown if the response of the Account Service has error
    /// </summary>
    [Serializable]
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
        /// Initializes a new instance of the <see cref="AccountServiceException"/> class
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected AccountServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets or sets the account service error code
        /// </summary>
        public EErrorCode ErrorCode { get; set; }
    }
}
