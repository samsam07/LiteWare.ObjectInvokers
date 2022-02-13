using LiteWare.ObjectInvokers.Attributes;
using LiteWare.ObjectInvokers.Exceptions;
using LiteWare.ObjectInvokers.Extensions;

namespace LiteWare.ObjectInvokers;

/// <summary>
/// Provides a mechanism that indirectly invoke members of an object by name.
/// </summary>
public class ObjectInvoker
{
    /// <summary>
    /// Builds an <see cref="ObjectInvoker"/> by binding the extracted members from <paramref name="contractType"/> to an object instance for indirect member invokes.
    /// </summary>
    /// <param name="contractType">The type on which members marked by the <see cref="InvokableMemberAttribute"/> will be extracted for member invokes.</param>
    /// <param name="targetInstance">The object instance on which member invokes will take place.</param>
    /// <returns>An <see cref="ObjectInvoker"/> that allow indirect member invokes.</returns>
    /// <remarks>Only members defining the <see cref="InvokableMemberAttribute"/> attribute will be extracted from <paramref name="contractType"/> for member invokes.</remarks>
    public static ObjectInvoker Bind(Type contractType, object targetInstance)
    {
        List<IObjectMember> objectMembers = contractType.FindInvokableMembers().ToList();
        return new ObjectInvoker(objectMembers, targetInstance);
    }

    /// <summary>
    /// Builds an <see cref="ObjectInvoker"/> by binding the extracted members from the type <typeparamref name="TContract"/> to an object instance for indirect member invokes.
    /// </summary>
    /// <typeparam name="TContract">The type on which members marked by the <see cref="InvokableMemberAttribute"/> will be extracted for member invokes.</typeparam>
    /// <param name="targetInstance">The object instance on which member invokes will take place.</param>
    /// <returns>An <see cref="ObjectInvoker"/> that allow indirect member invokes.</returns>
    /// <remarks>Only members defining the <see cref="InvokableMemberAttribute"/> attribute will be extracted from the type <typeparamref name="TContract"/> for member invokes.</remarks>
    public static ObjectInvoker Bind<TContract>(object targetInstance) =>
        Bind(typeof(TContract), targetInstance);

    /// <summary>
    /// Builds an <see cref="ObjectInvoker"/> by binding the extracted members from the type <typeparamref name="TContract"/> to an object instance for indirect member invokes.
    /// </summary>
    /// <typeparam name="TContract">The type on which members marked by the <see cref="InvokableMemberAttribute"/> will be extracted for member invokes.</typeparam>
    /// <param name="targetInstance">The object instance on which member invokes will take place.</param>
    /// <returns>An <see cref="ObjectInvoker"/> that allow indirect member invokes.</returns>
    /// <remarks>Only members defining the <see cref="InvokableMemberAttribute"/> attribute will be extracted from the type <typeparamref name="TContract"/> for member invokes.</remarks>
    public static ObjectInvoker Bind<TContract>(TContract targetInstance) where TContract : notnull =>
        Bind(typeof(TContract), targetInstance);

    private readonly List<IObjectMember> _objectMembers;

    /// <summary>
    /// Gets the object instance on which member invokes will take place.
    /// </summary>
    public object TargetInstance { get; }

    /// <summary>
    /// Gets a readonly collection of all the invokable members.
    /// </summary>
    public IReadOnlyCollection<IObjectMember> Members =>
        _objectMembers.AsReadOnly();

    /// <summary>
    /// Gets a readonly collection of all the invokable methods.
    /// </summary>
    public IReadOnlyCollection<ObjectMethod> Methods =>
        _objectMembers
            .Where(m => m is ObjectMethod)
            .Cast<ObjectMethod>()
            .ToList()
            .AsReadOnly();

    /// <summary>
    /// Gets a readonly collection of all the invokable properties.
    /// </summary>
    public IReadOnlyCollection<ObjectProperty> Properties =>
        _objectMembers
            .Where(m => m is ObjectProperty)
            .Cast<ObjectProperty>()
            .ToList()
            .AsReadOnly();

    /// <summary>
    /// Gets a readonly collection of all the invokable fields.
    /// </summary>
    public IReadOnlyCollection<ObjectField> Fields =>
        _objectMembers
            .Where(m => m is ObjectField)
            .Cast<ObjectField>()
            .ToList()
            .AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectInvoker"/> class with the provided list of invokable members for the provided object instance.
    /// </summary>
    /// <param name="objectMembers">A list of invokable members.</param>
    /// <param name="targetInstance">The object instance on which member invokes will take place.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="objectMembers"/> is <code>null</code>.</exception>
    public ObjectInvoker(List<IObjectMember> objectMembers, object targetInstance)
    {
        _objectMembers = objectMembers ?? throw new ArgumentNullException(nameof(objectMembers));
        TargetInstance = targetInstance;
    }

    /// <summary>
    /// Finds the best overloaded member matching the provided arguments and invokes its intended function.
    /// </summary>
    /// <param name="memberName">The name of the member to find and invoke.</param>
    /// <param name="genericTypes">Any generic types used in the invoke process.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    /// <exception cref="MemberNotFoundException">No member or member overloads were found for the provided arguments.</exception>
    /// <exception cref="AmbiguousMemberInvokeException">Unable to determine which member to invoke because more than one member overload were found for the provided arguments.</exception>
    public object? Invoke(string memberName, Type[]? genericTypes, object?[]? parameters)
    {
        int genericTypeCount = genericTypes?.Length ?? 0;
        var overloadedmemberNames = _objectMembers
            .Select(m => (Member: m, DeviancyScore: m.CalculateSignatureDeviancyScore(memberName, genericTypeCount, parameters)))
            .Where(kv => kv.DeviancyScore != MemberSignature.NoMatchScore)
            .ToList();

        IObjectMember[] bestOverloadedmemberNames = Array.Empty<IObjectMember>();
        if (overloadedmemberNames.Any())
        {
            int bestScore = overloadedmemberNames.Min(kv => kv.DeviancyScore);
            bestOverloadedmemberNames = overloadedmemberNames
                .Where(kv => kv.DeviancyScore == bestScore)
                .Select(kv => kv.Member)
                .ToArray();
        }

        if (bestOverloadedmemberNames.Length == 0)
        {
            throw new MemberNotFoundException($"Object member '{memberName}' for the provided arguments was not found.");
        }

        if (bestOverloadedmemberNames.Length == 1)
        {
            return bestOverloadedmemberNames
                .First()
                .Invoke(TargetInstance, genericTypes, parameters);
        }

        throw new AmbiguousMemberInvokeException(memberName) { AmbiguousMembers = bestOverloadedmemberNames };
    }

    /// <summary>
    /// Finds the best overloaded member matching the provided arguments and invokes its intended function.
    /// </summary>
    /// <param name="memberName">The name of the member to find and invoke.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    /// <exception cref="MemberNotFoundException">No member or member overloads were found for the provided arguments.</exception>
    /// <exception cref="AmbiguousMemberInvokeException">Unable to determine which member to invoke because more than one member overload were found for the provided arguments.</exception>
    public object? Invoke(string memberName, params object?[] parameters) =>
        Invoke(memberName, null, parameters);

    /// <summary>
    /// Finds the best overloaded member matching the provided generic type and arguments and invokes its intended function.
    /// </summary>
    /// <typeparam name="T">The generic type required by the invoke process.</typeparam>
    /// <param name="memberName">The name of the member to find and invoke.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    /// <exception cref="MemberNotFoundException">No member or member overloads were found for the provided generic type and arguments.</exception>
    /// <exception cref="AmbiguousMemberInvokeException">Unable to determine which member to invoke because more than one member overload were found for the provided generic type and arguments.</exception>
    public object? Invoke<T>(string memberName, params object?[] parameters) =>
        Invoke(memberName, new[] { typeof(T) }, parameters);

    /// <summary>
    /// Finds the best overloaded member matching the provided generic types and arguments and invokes its intended function.
    /// </summary>
    /// <typeparam name="T1">The first generic types required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic types required by the invoke process.</typeparam>
    /// <param name="memberName">The name of the member to find and invoke.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    /// <exception cref="MemberNotFoundException">No member or member overloads were found for the provided generic types and arguments.</exception>
    /// <exception cref="AmbiguousMemberInvokeException">Unable to determine which member to invoke because more than one member overload were found for the provided generic types and arguments.</exception>
    public object? Invoke<T1, T2>(string memberName, params object?[] parameters) =>
        Invoke(memberName, new[] { typeof(T1), typeof(T2) }, parameters);

    /// <summary>
    /// Finds the best overloaded member matching the provided generic types and arguments and invokes its intended function.
    /// </summary>
    /// <typeparam name="T1">The first generic type required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic type required by the invoke process.</typeparam>
    /// <typeparam name="T3">The third generic type required by the invoke process.</typeparam>
    /// <param name="memberName">The name of the member to find and invoke.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    /// <exception cref="MemberNotFoundException">No member or member overloads were found for the provided generic types and arguments.</exception>
    /// <exception cref="AmbiguousMemberInvokeException">Unable to determine which member to invoke because more than one member overload were found for the provided generic types and arguments.</exception>
    public object? Invoke<T1, T2, T3>(string memberName, params object?[] parameters) =>
        Invoke(memberName, new[] { typeof(T1), typeof(T2), typeof(T3) }, parameters);

    /// <summary>
    /// Finds the best overloaded member matching the provided generic types and arguments and invokes its intended function.
    /// </summary>
    /// <typeparam name="T1">The first generic type required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic type required by the invoke process.</typeparam>
    /// <typeparam name="T3">The third generic type required by the invoke process.</typeparam>
    /// <typeparam name="T4">The fourth generic type required by the invoke process.</typeparam>
    /// <param name="memberName">The name of the member to find and invoke.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    /// <exception cref="MemberNotFoundException">No member or member overloads were found for the provided generic types and arguments.</exception>
    /// <exception cref="AmbiguousMemberInvokeException">Unable to determine which member to invoke because more than one member overload were found for the provided generic types and arguments.</exception>
    public object? Invoke<T1, T2, T3, T4>(string memberName, params object?[] parameters) =>
        Invoke(memberName, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, parameters);

    /// <summary>
    /// Finds the best overloaded member matching the provided generic types and arguments and invokes its intended function.
    /// </summary>
    /// <typeparam name="T1">The first generic type required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic type required by the invoke process.</typeparam>
    /// <typeparam name="T3">The third generic type required by the invoke process.</typeparam>
    /// <typeparam name="T4">The fourth generic type required by the invoke process.</typeparam>
    /// <typeparam name="T5">The fifth generic type required by the invoke process.</typeparam>
    /// <param name="memberName">The name of the member to find and invoke.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    /// <exception cref="MemberNotFoundException">No member or member overloads were found for the provided generic types and arguments.</exception>
    /// <exception cref="AmbiguousMemberInvokeException">Unable to determine which member to invoke because more than one member overload were found for the provided generic types and arguments.</exception>
    public object? Invoke<T1, T2, T3, T4, T5>(string memberName, params object?[] parameters) =>
        Invoke(memberName, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, parameters);

    /// <summary>
    /// Finds the best overloaded member matching the provided generic types and arguments and invokes its intended function.
    /// </summary>
    /// <typeparam name="T1">The first generic type required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic type required by the invoke process.</typeparam>
    /// <typeparam name="T3">The third generic type required by the invoke process.</typeparam>
    /// <typeparam name="T4">The fourth generic type required by the invoke process.</typeparam>
    /// <typeparam name="T5">The fifth generic type required by the invoke process.</typeparam>
    /// <typeparam name="T6">The sixth generic type required by the invoke process.</typeparam>
    /// <param name="memberName">The name of the member to find and invoke.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    /// <exception cref="MemberNotFoundException">No member or member overloads were found for the provided generic types and arguments.</exception>
    /// <exception cref="AmbiguousMemberInvokeException">Unable to determine which member to invoke because more than one member overload were found for the provided generic types and arguments.</exception>
    public object? Invoke<T1, T2, T3, T4, T5, T6>(string memberName, params object?[] parameters) =>
        Invoke(memberName, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, parameters);
}