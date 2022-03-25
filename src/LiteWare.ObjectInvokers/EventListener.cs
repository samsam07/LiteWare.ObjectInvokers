using System.Reflection;
using LiteWare.ObjectInvokers.Attributes;
using LiteWare.ObjectInvokers.Extensions;

namespace LiteWare.ObjectInvokers;

/// <summary>
/// Provides a mechanism that listens for the raising of events on an object.
/// </summary>
public class EventListener : IDisposable
{
    /// <summary>
    /// Builds an <see cref="EventListener"/> by binding the extracted events from <paramref name="contractType"/> to an object instance for event raise monitoring.
    /// </summary>
    /// <param name="contractType">The type on which events will be extracted for event raise monitoring.</param>
    /// <param name="targetInstance">The object instance on which event raise monitoring will take place.</param>
    /// <param name="eventMatch">A predicate to filter events from <paramref name="contractType"/>.</param>
    /// <param name="preferredNameSelector">A function that returns the preferred name of a specific event.</param>
    /// <param name="eventNotifiers">A collection of <see cref="IEventNotifier"/> which will notify when events are raised.</param>
    /// <returns>An <see cref="EventListener"/> that allow event raise monitoring.</returns>
    public static EventListener Bind(Type contractType, object targetInstance, Predicate<EventInfo> eventMatch, Func<EventInfo, string?>? preferredNameSelector = null, params IEventNotifier[] eventNotifiers)
    {
        EventBroadcaster eventBroadcaster = new(eventNotifiers);
        Dictionary<EventInfo, Delegate> eventSubscriberDelegates = contractType.ExtractEventSubscriberDelegates(eventBroadcaster, eventMatch, preferredNameSelector);

        return new EventListener(eventSubscriberDelegates, targetInstance);
    }

    /// <summary>
    /// Builds an <see cref="EventListener"/> by binding the extracted events from <paramref name="contractType"/> to an object instance for event raise monitoring.
    /// </summary>
    /// <param name="contractType">The type on which events marked by <see cref="ListenableAttribute"/> will be extracted for event raise monitoring.</param>
    /// <param name="targetInstance">The object instance on which event raise monitoring will take place.</param>
    /// <param name="eventNotifiers">A collection of <see cref="IEventNotifier"/> which will notify when events are raised.</param>
    /// <returns>An <see cref="EventListener"/> that allow event raise monitoring.</returns>
    /// <remarks>Only events defining the <see cref="ListenableAttribute"/> will be extracted from <paramref name="contractType"/> for event raise monitoring.</remarks>
    public static EventListener Bind(Type contractType, object targetInstance, params IEventNotifier[] eventNotifiers)
    {
        EventBroadcaster eventBroadcaster = new(eventNotifiers);
        Dictionary<EventInfo, Delegate> eventSubscriberDelegates = contractType.FindEventSubscriberDelegates(eventBroadcaster);

        return new EventListener(eventSubscriberDelegates, targetInstance);
    }

    /// <summary>
    /// Builds an <see cref="EventListener"/> by binding the extracted events from the type <typeparamref name="TContract"/> to an object instance for event raise monitoring.
    /// </summary>
    /// <typeparam name="TContract">The type on which events will be extracted for event raise monitoring.</typeparam>
    /// <param name="targetInstance">The object instance on which event raise monitoring will take place.</param>
    /// <param name="eventMatch">A predicate to filter events from the type <typeparamref name="TContract"/>.</param>
    /// <param name="preferredNameSelector">A function that returns the preferred name of a specific event.</param>
    /// <param name="eventNotifiers">A collection of <see cref="IEventNotifier"/> which will notify when events are raised.</param>
    /// <returns>An <see cref="EventListener"/> that allow event raise monitoring.</returns>
    public static EventListener Bind<TContract>(object targetInstance, Predicate<EventInfo> eventMatch, Func<EventInfo, string?>? preferredNameSelector = null, params IEventNotifier[] eventNotifiers) =>
        Bind(typeof(TContract), targetInstance, eventMatch, preferredNameSelector, eventNotifiers);

    /// <summary>
    /// Builds an <see cref="EventListener"/> by binding the extracted events from the type <typeparamref name="TContract"/> to an object instance for event raise monitoring.
    /// </summary>
    /// <typeparam name="TContract">The type on which events marked by the <see cref="ListenableAttribute"/> will be extracted for event raise monitoring.</typeparam>
    /// <param name="targetInstance">The object instance on which event raise monitoring will take place.</param>
    /// <param name="eventNotifiers">A collection of <see cref="IEventNotifier"/> which will notify when events are raised.</param>
    /// <returns>An <see cref="EventListener"/> that allow event raise monitoring.</returns>
    /// <remarks>Only events defining the <see cref="ListenableAttribute"/> will be extracted from the type <typeparamref name="TContract"/> for event raise monitoring.</remarks>
    public static EventListener Bind<TContract>(object targetInstance, params IEventNotifier[] eventNotifiers) =>
        Bind(typeof(TContract), targetInstance, eventNotifiers);

    /// <summary>
    /// Builds an <see cref="EventListener"/> by binding the extracted events from the type <typeparamref name="TContract"/> to an object instance for event raise monitoring.
    /// </summary>
    /// <typeparam name="TContract">The type on which events will be extracted for event raise monitoring.</typeparam>
    /// <param name="targetInstance">The object instance on which event raise monitoring will take place.</param>
    /// <param name="eventMatch">A predicate to filter events from the type <typeparamref name="TContract"/>.</param>
    /// <param name="preferredNameSelector">A function that returns the preferred name of a specific event.</param>
    /// <param name="eventNotifiers">A collection of <see cref="IEventNotifier"/> which will notify when events are raised.</param>
    /// <returns>An <see cref="EventListener"/> that allow event raise monitoring.</returns>
    public static EventListener Bind<TContract>(TContract targetInstance, Predicate<EventInfo> eventMatch, Func<EventInfo, string?>? preferredNameSelector = null, params IEventNotifier[] eventNotifiers) where TContract : notnull =>
        Bind(typeof(TContract), targetInstance, eventMatch, preferredNameSelector, eventNotifiers);

    /// <summary>
    /// Builds an <see cref="EventListener"/> by binding the extracted events from the type <typeparamref name="TContract"/> to an object instance for event raise monitoring.
    /// </summary>
    /// <typeparam name="TContract">The type on which events marked by the <see cref="ListenableAttribute"/> will be extracted for event raise monitoring.</typeparam>
    /// <param name="targetInstance">The object instance on which event raise monitoring will take place.</param>
    /// <param name="eventNotifiers">A collection of <see cref="IEventNotifier"/> which will notify when events are raised.</param>
    /// <returns>An <see cref="EventListener"/> that allow event raise monitoring.</returns>
    /// <remarks>Only events defining the <see cref="ListenableAttribute"/> will be extracted from the type <typeparamref name="TContract"/> for event raise monitoring.</remarks>
    public static EventListener Bind<TContract>(TContract targetInstance, params IEventNotifier[] eventNotifiers) where TContract : notnull =>
        Bind(typeof(TContract), targetInstance, eventNotifiers);

    private readonly Dictionary<EventInfo, Delegate> _eventSubscriberDelegates;

    /// <summary>
    /// Gets the object instance on which the raising of events will take place.
    /// </summary>
    public object TargetInstance { get; }

    /// <summary>
    /// Gets a value indicating whether the raising of events is being monitored.
    /// </summary>
    public bool IsListening { get; private set; }

    internal EventListener(Dictionary<EventInfo, Delegate> eventSubscriberDelegates, object targetInstance)
    {
        _eventSubscriberDelegates = eventSubscriberDelegates;
        TargetInstance = targetInstance;
    }

    /// <summary>
    /// Starts the monitoring of raising of events.
    /// </summary>
    public void StartListening()
    {
        if (IsListening)
        {
            return;
        }

        foreach ((EventInfo eventInfo, Delegate subscriberDelegate) in _eventSubscriberDelegates)
        {
            eventInfo.AddEventHandler(TargetInstance, subscriberDelegate);
        }

        IsListening = true;
    }

    /// <summary>
    /// Stops the monitoring of raising of events.
    /// </summary>
    public void StopListening()
    {
        if (!IsListening)
        {
            return;
        }

        foreach ((EventInfo eventInfo, Delegate subscriberDelegate) in _eventSubscriberDelegates)
        {
            eventInfo.RemoveEventHandler(TargetInstance, subscriberDelegate);
        }

        IsListening = false;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        StopListening();
        _eventSubscriberDelegates.Clear();

        GC.SuppressFinalize(this);
    }
}