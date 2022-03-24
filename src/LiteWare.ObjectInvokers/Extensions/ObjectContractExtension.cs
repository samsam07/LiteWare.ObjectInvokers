using System.Linq.Expressions;
using System.Reflection;
using LiteWare.ObjectInvokers.Attributes;

namespace LiteWare.ObjectInvokers.Extensions;

/// <summary>
/// Provide extension methods for object member extraction.
/// </summary>
public static class ObjectContractExtension
{
    private const string DelegateInvokeMethodName = "Invoke";

    /// <summary>
    /// Represents the supported binding flags used in object member extraction through reflection.
    /// </summary>
    public const BindingFlags SupportedMemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static;

    private static readonly MethodInfo NotifyEventMethodInfo = typeof(IEventNotifier).GetMethod(nameof(IEventNotifier.NotifyEvent))!;

    /// <summary>
    /// Finds all members of <paramref name="contractType"/> matching the provided predicate and transform them into instances of <see cref="IObjectMember"/> using preferred name obtained from <paramref name="preferredNameSelector"/>.
    /// </summary>
    /// <param name="contractType">The type on which members will be extracted.</param>
    /// <param name="memberMatch">A predicate to filter members from <paramref name="contractType"/>.</param>
    /// <param name="preferredNameSelector">A function that returns the preferred name of a specific member.</param>
    /// <returns>A transformed list of <see cref="IObjectMember"/>.</returns>
    /// <exception cref="NotSupportedException">The <paramref name="contractType"/> contains unsupported members marked with <see cref="InvokableAttribute"/>.</exception>
    public static IEnumerable<IObjectMember> ExtractInvokableMembers(this Type contractType, Predicate<MemberInfo>? memberMatch = null, Func<MemberInfo, string?>? preferredNameSelector = null) =>
        contractType
            .GetMembers(SupportedMemberBindingFlags)
            .Where(m => memberMatch?.Invoke(m) ?? true)
            .Select(m => m.ToObjectMember(preferredNameSelector?.Invoke(m)));

    /// <summary>
    /// Finds all members of <paramref name="contractType"/> marked with <see cref="InvokableAttribute"/> and transform them into instances of <see cref="IObjectMember"/>.
    /// </summary>
    /// <param name="contractType">The type on which members marked by the <see cref="InvokableAttribute"/> will be extracted.</param>
    /// <returns>A transformed list of <see cref="IObjectMember"/>.</returns>
    /// <exception cref="NotSupportedException">The <paramref name="contractType"/> contains unsupported members marked with <see cref="InvokableAttribute"/>.</exception>
    public static IEnumerable<IObjectMember> FindInvokableMembers(this Type contractType) =>
        contractType.ExtractInvokableMembers
        (
            m => Attribute.IsDefined(m, typeof(InvokableAttribute)),
            m => m.GetCustomAttribute<InvokableAttribute>(true)?.PreferredName
        );

    /// <summary>
    /// Converts a member info into an instance of <see cref="IObjectMember"/>.
    /// </summary>
    /// <param name="contractMemberInfo">The member info.</param>
    /// <param name="preferredName">The preferred object member name.</param>
    /// <returns>An instance of <see cref="IObjectMember"/>.</returns>
    /// <exception cref="NotSupportedException">The <paramref name="contractMemberInfo"/> member is not supported and cannot be converted to an instance of <see cref="IObjectMember"/>.</exception>
    public static IObjectMember ToObjectMember(this MemberInfo contractMemberInfo, string? preferredName = null) =>
        contractMemberInfo switch
        {
            MethodInfo methodInfo => new ObjectMethod(methodInfo, preferredName),
            PropertyInfo propertyInfo => new ObjectProperty(propertyInfo, preferredName),
            FieldInfo fieldInfo => new ObjectField(fieldInfo, preferredName),
            _ => throw new NotSupportedException($"Member of the type '{contractMemberInfo.GetType().Name}' is not supported.")
        };

    /// <summary>
    /// Finds all events of <paramref name="contractType"/> matching the provided predicate and create a list of event subscriber delegates using the preferred name obtained from <paramref name="preferredNameSelector"/>.
    /// </summary>
    /// <param name="contractType">The type on which events will be extracted.</param>
    /// <param name="eventNotifier">The event notifier which will be used to create the event subscriber delegates.</param>
    /// <param name="eventMatch">A predicate to filter events from <paramref name="contractType"/>.</param>
    /// <param name="preferredNameSelector">A function that returns the preferred name of a specific event.</param>
    /// <returns>A dictionary of the event info with its subscriber delegate.</returns>
    public static Dictionary<EventInfo, Delegate> ExtractEventSubscriberDelegates(this Type contractType, IEventNotifier eventNotifier, Predicate<EventInfo>? eventMatch = null, Func<EventInfo, string?>? preferredNameSelector = null) =>
        contractType
            .GetEvents(SupportedMemberBindingFlags)
            .Where(m => eventMatch?.Invoke(m) ?? true)
            .ToDictionary
            (
                e => e,
                e => e.CreateSubscriberDelegate(eventNotifier, preferredNameSelector?.Invoke(e))
            );

    /// <summary>
    /// Finds all events of <paramref name="contractType"/> marked with <see cref="ListenableAttribute"/> and create a list of event subscriber delegates.
    /// </summary>
    /// <param name="contractType">The type on which events marked with <see cref="ListenableAttribute"/> will be extracted.</param>
    /// <param name="eventNotifier">The event notifier which will be used to create the event subscriber delegates.</param>
    /// <returns>A dictionary of the event info with its subscriber delegate.</returns>
    public static Dictionary<EventInfo, Delegate> FindEventSubscriberDelegates(this Type contractType, IEventNotifier eventNotifier) =>
        contractType.ExtractEventSubscriberDelegates
        (
            eventNotifier,
            e => Attribute.IsDefined(e, typeof(ListenableAttribute)),
            e => e.GetCustomAttribute<ListenableAttribute>(true)?.PreferredName
        );

    /// <summary>
    /// Creates an event subscriber delegate that notifies raised event through <paramref name="eventNotifier"/>.
    /// </summary>
    /// <param name="eventInfo">The event info.</param>
    /// <param name="eventNotifier">The event notifier that will notify a raised event.</param>
    /// <param name="preferredName">The preferred event name.</param>
    /// <returns>A subscriber delegate.</returns>
    public static Delegate CreateSubscriberDelegate(this EventInfo eventInfo, IEventNotifier eventNotifier, string? preferredName = null)
    {
        /* ♥♥♥ Inspired from https://stackoverflow.com/a/45901/5240378 ♥♥♥ */

        List<ParameterExpression> parametersExpression = eventInfo
            .EventHandlerType
            ?.GetMethod(DelegateInvokeMethodName)
            ?.GetParameters()
            .Select(p => Expression.Parameter(p.ParameterType))
            .ToList() ?? new List<ParameterExpression>();

        IEnumerable<UnaryExpression> arrayExpression = parametersExpression.Select(p => Expression.Convert(p, typeof(object)));
        ConstantExpression eventNotifierExpression = Expression.Constant(eventNotifier);
        ConstantExpression eventNameExpression = Expression.Constant(preferredName ?? eventInfo.Name);
        NewArrayExpression eventArgumentsExpression = Expression.NewArrayInit(typeof(object), arrayExpression);

        // Lambda: (<param_1>, ..., <param_n>) => eventNotifier.NotifyEvent(<event_name>, new object[] { (object)<param_1>, ..., (object)<param_n> })
        MethodCallExpression body = Expression.Call(eventNotifierExpression, NotifyEventMethodInfo, eventNameExpression, eventArgumentsExpression);
        LambdaExpression lambdaExpression = Expression.Lambda(body, parametersExpression);

        return Delegate.CreateDelegate(eventInfo.EventHandlerType!, lambdaExpression.Compile(), DelegateInvokeMethodName);
    }
}