namespace LiteWare.ObjectInvokers.Exceptions;

/// <summary>
/// Exception that is thrown when a specific member could not be found.
/// </summary>
[Serializable]
public class MemberNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberNotFoundException"/> class.
    /// </summary>
    public MemberNotFoundException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberNotFoundException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for this exception.</param>
    public MemberNotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberNotFoundException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for this exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public MemberNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}