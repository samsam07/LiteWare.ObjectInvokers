using System.Reflection;
using LiteWare.ObjectInvokers.Utilities;

namespace LiteWare.ObjectInvokers;

/// <summary>
/// Base class that represents an object's member.
/// </summary>
/// <typeparam name="TMemberInfo">A derivation of the <see cref="MemberInfo"/> type containing information about the object's member.</typeparam>
public abstract class ObjectMember<TMemberInfo> : IObjectMember where TMemberInfo : MemberInfo
{
    /// <summary>
    /// Gets information of the object's member.
    /// </summary>
    public TMemberInfo TargetMemberInfo { get; }

    /// <summary>
    /// Gets the signature of the object's member.
    /// </summary>
    public MemberSignature Signature { get; }

    /// <summary>
    /// Gets the preferred object's member name.
    /// </summary>
    public string PreferredName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectMember{TMemberInfo}"/> class with the provided member info.
    /// </summary>
    /// <param name="memberInfo">A derivation of <see cref="MemberInfo"/> that contains information about the object's member.</param>
    /// <param name="preferredName">The preferred object's member name. Defaults to the original member name if not provided.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="memberInfo"/> is <code>null</code>.</exception>
    protected ObjectMember(TMemberInfo memberInfo, string? preferredName = null)
    {
        TargetMemberInfo = memberInfo ?? throw new ArgumentNullException(nameof(memberInfo));
        Signature = BuildSignature(memberInfo);
        PreferredName = preferredName ?? Signature.Name;
    }

    /// <summary>
    /// When implemented in a derived class, builds the member's signature.
    /// </summary>
    /// <param name="memberInfo">Information about the member.</param>
    /// <returns>The signature of the member.</returns>
    protected abstract MemberSignature BuildSignature(TMemberInfo memberInfo);

    /// <summary>
    /// Calculates a score of how much the provided arguments deviates from the member's method signature.
    /// </summary>
    /// <param name="memberName">The member name used in an invoke process.</param>
    /// <param name="genericTypeCount">The number of generic types used in an invoke process.</param>
    /// <param name="parameters">The parameters used in an invoke process.</param>
    /// <returns>A score indicating how much the provided arguments deviates from the member's method signature.</returns>
    /// <remarks>
    /// A score of 0 means the supplied arguments are a perfect method signature match.
    /// A score above 0 means compatible signature and indicates how much the supplied arguments deviate from the member's method signature, usually through optional parameters.
    /// </remarks>
    public int CalculateSignatureDeviancyScore(string memberName, int genericTypeCount, object?[]? parameters) =>
        SignatureDeviancyCalculatorUtility.CalculateDeviancyScore(this, memberName, genericTypeCount, parameters);

    /// <summary>
    /// When implemented in a derived class, invokes and executes the intended function of the member on <paramref name="objectInstance"/> with the provided generic types and parameters.
    /// </summary>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="genericTypes">Any generic types used in the invoke process.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    public abstract object? Invoke(object objectInstance, Type[]? genericTypes, object?[]? parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with the provided parameters.
    /// </summary>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    public object? Invoke(object objectInstance, params object?[] parameters) =>
        Invoke(objectInstance, null, parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with a generic type and the provided parameters.
    /// </summary>
    /// <typeparam name="T">The generic type required by the invoke process.</typeparam>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    public object? Invoke<T>(object objectInstance, params object?[] parameters) =>
        Invoke(objectInstance, new[] { typeof(T) }, parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with two generic type and the provided parameters.
    /// </summary>
    /// <typeparam name="T1">The first generic type required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic type required by the invoke process.</typeparam>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    public object? Invoke<T1, T2>(object objectInstance, params object?[] parameters) =>
        Invoke(objectInstance, new[] { typeof(T1), typeof(T2) }, parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with three generic type and the provided parameters.
    /// </summary>
    /// <typeparam name="T1">The first generic type required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic type required by the invoke process.</typeparam>
    /// <typeparam name="T3">The third generic type required by the invoke process.</typeparam>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    public object? Invoke<T1, T2, T3>(object objectInstance, params object?[] parameters) =>
        Invoke(objectInstance, new[] { typeof(T1), typeof(T2), typeof(T3) }, parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with four generic type and the provided parameters.
    /// </summary>
    /// <typeparam name="T1">The first generic type required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic type required by the invoke process.</typeparam>
    /// <typeparam name="T3">The third generic type required by the invoke process.</typeparam>
    /// <typeparam name="T4">The fourth generic type required by the invoke process.</typeparam>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    public object? Invoke<T1, T2, T3, T4>(object objectInstance, params object?[] parameters) =>
        Invoke(objectInstance, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with five generic type and the provided parameters.
    /// </summary>
    /// <typeparam name="T1">The first generic type required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic type required by the invoke process.</typeparam>
    /// <typeparam name="T3">The third generic type required by the invoke process.</typeparam>
    /// <typeparam name="T4">The fourth generic type required by the invoke process.</typeparam>
    /// <typeparam name="T5">The fifth generic type required by the invoke process.</typeparam>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    public object? Invoke<T1, T2, T3, T4, T5>(object objectInstance, params object?[] parameters) =>
        Invoke(objectInstance, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with six generic type and the provided parameters.
    /// </summary>
    /// <typeparam name="T1">The first generic type required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic type required by the invoke process.</typeparam>
    /// <typeparam name="T3">The third generic type required by the invoke process.</typeparam>
    /// <typeparam name="T4">The fourth generic type required by the invoke process.</typeparam>
    /// <typeparam name="T5">The fifth generic type required by the invoke process.</typeparam>
    /// <typeparam name="T6">The sixth generic type required by the invoke process.</typeparam>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    public object? Invoke<T1, T2, T3, T4, T5, T6>(object objectInstance, params object?[] parameters) =>
        Invoke(objectInstance, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, parameters);
}