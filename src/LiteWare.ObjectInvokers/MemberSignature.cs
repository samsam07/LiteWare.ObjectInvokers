namespace LiteWare.ObjectInvokers;

/// <summary>
/// Represent the signature of an object's member.
/// </summary>
public record MemberSignature(string Name, int GenericTypeCount, MemberParameter[] Parameters)
{
    /// <summary>
    /// Represents a no-match signature deviancy score.
    /// </summary>
    public const int NoMatchScore = int.MaxValue;

    ///<summary>
    /// Gets the object's member name.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets the number of generic types defined by the the object's member.
    /// </summary>
    public int GenericTypeCount { get; init; } = GenericTypeCount;

    /// <summary>
    /// Gets the input parameters of the object's member.
    /// </summary>
    public MemberParameter[] Parameters { get; init; } = Parameters;
}