namespace LiteWare.ObjectInvokers;

/// <summary>
/// Represents an object's member.
/// </summary>
public interface IObjectMember
{
    /// <summary>
    /// Gets the signature of the object's member.
    /// </summary>
    MemberSignature Signature { get; }

    /// <summary>
    /// Gets the preferred object's member name.
    /// </summary>
    string PreferredName { get; }

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
    int CalculateSignatureDeviancyScore(string memberName, int genericTypeCount, object?[]? parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with the provided generic types and parameters.
    /// </summary>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="genericTypes">Any generic types used in the invoke process.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    object? Invoke(object objectInstance, Type[]? genericTypes, object?[]? parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with the provided parameters.
    /// </summary>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    object? Invoke(object objectInstance, params object?[] parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with a generic type and the provided parameters.
    /// </summary>
    /// <typeparam name="T">The generic type required by the invoke process.</typeparam>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    object? Invoke<T>(object objectInstance, params object?[] parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with two generic type and the provided parameters.
    /// </summary>
    /// <typeparam name="T1">The first generic type required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic type required by the invoke process.</typeparam>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    object? Invoke<T1, T2>(object objectInstance, params object?[] parameters);

    /// <summary>
    /// Invokes and executes the intended function of the member on <paramref name="objectInstance"/> with three generic type and the provided parameters.
    /// </summary>
    /// <typeparam name="T1">The first generic type required by the invoke process.</typeparam>
    /// <typeparam name="T2">The second generic type required by the invoke process.</typeparam>
    /// <typeparam name="T3">The third generic type required by the invoke process.</typeparam>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    object? Invoke<T1, T2, T3>(object objectInstance, params object?[] parameters);

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
    object? Invoke<T1, T2, T3, T4>(object objectInstance, params object?[] parameters);

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
    object? Invoke<T1, T2, T3, T4, T5>(object objectInstance, params object?[] parameters);

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
    object? Invoke<T1, T2, T3, T4, T5, T6>(object objectInstance, params object?[] parameters);
}