namespace TiCodeX.SQLSchemaCompare.Core.Entities.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The exception that is thrown when a property cannot be mapped during a query
    /// </summary>
    [Serializable]
    public class PropertyNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNotFoundException"/> class.
        /// </summary>
        public PropertyNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public PropertyNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The exception that is cause of the current exception</param>
        public PropertyNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="classType">The type that's missing the property</param>
        /// <param name="propertyName">The missing property name</param>
        /// <param name="innerException">The exception that is cause of the current exception</param>
        public PropertyNotFoundException(string message, Type classType, string propertyName, Exception innerException)
            : base(message, innerException)
        {
            this.ClassType = classType;
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="classType">The type that's missing the property</param>
        /// <param name="propertyName">The missing property name</param>
        public PropertyNotFoundException(string message, Type classType, string propertyName)
            : base(message)
        {
            this.ClassType = classType;
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNotFoundException"/> class.
        /// </summary>
        /// <param name="classType">The type that's missing the property</param>
        /// <param name="propertyName">The missing property name</param>
        public PropertyNotFoundException(Type classType, string propertyName)
            : base("The property has not been found in the class.")
        {
            this.ClassType = classType;
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNotFoundException"/> class
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051")]
        protected PropertyNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc/>
        public override string Message
        {
            get
            {
                if (this.ClassType != null)
                {
                    return base.Message + Environment.NewLine + $"Class: {this.ClassType.Name}; Property: {this.PropertyName}";
                }

                return base.Message;
            }
        }

        /// <summary>
        /// Gets the class that's missing the property
        /// </summary>
        public Type ClassType { get; }

        /// <summary>
        /// Gets the missing property name
        /// </summary>
        public string PropertyName { get; }
    }
}
