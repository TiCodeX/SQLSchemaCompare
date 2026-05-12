namespace TiCodeX.SQLSchemaCompare.CLI.Utils;

using System.Runtime.Serialization;

/// <summary>
/// The show help exception
/// </summary>
[Serializable]
public class ShowHelpException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowHelpException"/> class
    /// </summary>
    public ShowHelpException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShowHelpException"/> class
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ShowHelpException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShowHelpException"/> class
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
    public ShowHelpException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShowHelpException"/> class
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051")]
    protected ShowHelpException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
