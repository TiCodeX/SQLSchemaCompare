namespace TiCodeX.SQLSchemaCompare.Core.Entities.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The exception that is thrown when the property cannot be mapped during a query because the type returned is wrong
    /// </summary>
    [Serializable]
    public class WrongTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WrongTypeException"/> class
        /// </summary>
        public WrongTypeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrongTypeException"/> class
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public WrongTypeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrongTypeException"/> class
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public WrongTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrongTypeException"/> class
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected WrongTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
