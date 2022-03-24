namespace LiteWare.ObjectInvokers;

/// <summary>
/// Provides a mechanism to broadcast a notification when an event is raised.
/// </summary>
public class EventBroadcaster : IEventNotifier
{
    /// <summary>
    /// Gets or sets the broadcast list of <see cref="EventBroadcaster"/>.
    /// </summary>
    public IList<IEventNotifier> EventNotifiers { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBroadcaster"/> class with the provided event notifiers.
    /// </summary>
    /// <param name="eventNotifiers">The broadcast collection of <see cref="IEventNotifier"/>.</param>
    public EventBroadcaster(params IEventNotifier[] eventNotifiers)
    {
        EventNotifiers = eventNotifiers;
    }

    /// <summary>
    /// Broadcasts that an event was raised.
    /// </summary>
    /// <param name="eventName">The name of the raised event.</param>
    /// <param name="arguments">The event arguments associated with the raised event.</param>
    public void NotifyEvent(string eventName, object?[] arguments)
    {
        foreach (IEventNotifier eventNotifier in EventNotifiers)
        {
            eventNotifier.NotifyEvent(eventName, arguments);
        }
    }
}