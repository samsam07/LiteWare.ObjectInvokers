namespace LiteWare.ObjectInvokers.Attributes;

/// <summary>
/// Indicates that that an object's method, property or field can be invoked.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class InvokableMemberAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the preferred object member name.
    /// </summary>
    public string? PreferredName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvokableMemberAttribute"/> class with the optionally provided preferred name.
    /// </summary>
    /// <param name="preferredName">The preferred object member name. Leave empty to use the original member name.</param>
    public InvokableMemberAttribute(string? preferredName = null)
    {
        PreferredName = preferredName;
    }
}