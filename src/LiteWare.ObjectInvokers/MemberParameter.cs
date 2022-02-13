namespace LiteWare.ObjectInvokers;

/// <summary>
/// Represents information about an object's member input parameter.
/// </summary>
public record MemberParameter(Type ParameterType, bool IsOptional = false, bool IsParamArray = false)
{
    /// <summary>
    /// Gets the <see cref="Type"/> of the parameter.
    /// </summary>
    public Type ParameterType { get; init; } = ParameterType;

    /// <summary>
    /// Gets a value indicating whether the parameter is optional.
    /// </summary>
    public bool IsOptional { get; init; } = IsOptional;

    /// <summary>
    /// Gets a value indicating whether the parameter will allow a variable number of arguments in its invocation.
    /// </summary>
    public bool IsParamArray { get; init; } = IsParamArray;

    /// <summary>
    /// Gets a value indicating whether the parameter type is a generic type.
    /// </summary>
    public bool IsGeneric => ParameterType.IsGenericParameter;
}