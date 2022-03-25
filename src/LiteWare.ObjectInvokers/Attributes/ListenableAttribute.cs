namespace LiteWare.ObjectInvokers.Attributes;

/// <summary>
/// Indicates that an event can be monitored for event raising operations.
/// </summary>
[AttributeUsage(AttributeTargets.Event)]
public sealed class ListenableAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the preferred event name.
    /// </summary>
    public string? PreferredName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListenableAttribute"/> class with the optionally provided preferred event name.
    /// </summary>
    /// <param name="preferredName">The preferred event name. Leave empty to use the original event name.</param>
    public ListenableAttribute(string? preferredName = null)
    {
        PreferredName = preferredName;
    }
}