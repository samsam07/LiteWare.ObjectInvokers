namespace LiteWare.ObjectInvokers.Exceptions;

/// <summary>
/// Exception that is thrown when the system is unable to determine which member invoke.
/// </summary>
[Serializable]
public class AmbiguousMemberInvokeException : Exception
{
    /// <summary>
    /// Gets an array of ambiguous object members.
    /// </summary>
    public IObjectMember[]? AmbiguousMembers { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousMemberInvokeException"/> class.
    /// </summary>
    public AmbiguousMemberInvokeException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousMemberInvokeException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for this exception.</param>
    public AmbiguousMemberInvokeException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousMemberInvokeException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for this exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public AmbiguousMemberInvokeException(string message, Exception inner) : base(message, inner)
    {
    }
}