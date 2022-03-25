namespace LiteWare.ObjectInvokers.Attributes;

/// <summary>
/// Indicates that an object's method, property or field can be invoked.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class InvokableAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the preferred object member name.
    /// </summary>
    public string? PreferredName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvokableAttribute"/> class with the optionally provided preferred name.
    /// </summary>
    /// <param name="preferredName">The preferred object member name. Leave empty to use the original member name.</param>
    public InvokableAttribute(string? preferredName = null)
    {
        PreferredName = preferredName;
    }
}