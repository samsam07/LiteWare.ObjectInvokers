namespace LiteWare.ObjectInvokers;

/// <summary>
/// Provides a mechanism to notify when an event is raised.
/// </summary>
public interface IEventNotifier
{
    /// <summary>
    /// Notifies that an event was raised.
    /// </summary>
    /// <param name="eventName">The name of the raised event.</param>
    /// <param name="arguments">The event arguments associated with the raised event.</param>
    void NotifyEvent(string eventName, object?[] arguments);
}