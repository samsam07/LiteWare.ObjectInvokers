using System.Reflection;

namespace LiteWare.ObjectInvokers;

/// <summary>
/// Represents an object's method.
/// </summary>
public class ObjectMethod : ObjectMember<MethodInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectMethod"/> class with the provided method info.
    /// </summary>
    /// <param name="methodInfo">Information about the method.</param>
    /// <param name="preferredName">The preferred method name. Defaults to the original method name if not provided.</param>
    public ObjectMethod(MethodInfo methodInfo, string? preferredName = null)
        : base(methodInfo, preferredName) { }

    /// <summary>
    /// Builds the member's signature.
    /// </summary>
    /// <param name="methodInfo">Information about the method.</param>
    /// <returns>The signature of the member.</returns>
    protected override MemberSignature BuildSignature(MethodInfo methodInfo)
    {
        List<MemberParameter> parameters = new();
        foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
        {
            if (parameterInfo.ParameterType.IsByRef)
            {
                throw new NotSupportedException("Parameters passed by ref/out keyword are not supported.");
            }

            bool isParamArray = parameterInfo.IsDefined(typeof(ParamArrayAttribute), false);
            MemberParameter parameter = new(parameterInfo.ParameterType, parameterInfo.IsOptional, isParamArray);

            parameters.Add(parameter);
        }

        return new MemberSignature(methodInfo.Name, methodInfo.GetGenericArguments().Length, parameters.ToArray());
    }

    /// <summary>
    /// Tries to transform the provided parameters to support the actual parameters from underlying method signature.
    /// </summary>
    /// <param name="parameters">The original parameters used for the invoke process.</param>
    /// <returns>A transformed array of parameters.</returns>
    private object?[] NormalizeParameters(object?[]? parameters)
    {
        List<object?> normalizedParameters = new();

        int providedParameterCount = parameters?.Length ?? 0;
        for (int i = 0; i < Signature.Parameters.Length; i++)
        {
            MemberParameter signatureParameter = Signature.Parameters[i];
            if (signatureParameter.IsParamArray)
            {
                Array paramsParameter = CreateParamsParameter(signatureParameter.ParameterType.GetElementType()!, parameters, i);
                normalizedParameters.Add(paramsParameter);

                return normalizedParameters.ToArray();
            }

            bool parameterProvided = (i < providedParameterCount);
            if (parameterProvided)
            {
                normalizedParameters.Add(parameters![i]);
            }
            else if (signatureParameter.IsOptional)
            {
                normalizedParameters.Add(Type.Missing);
            }
        }

        // Adds any excessive parameters
        if (providedParameterCount > Signature.Parameters.Length)
        {
            int excessiveParameterStartIndex = Signature.Parameters.Length;
            normalizedParameters.AddRange(parameters![excessiveParameterStartIndex..]);
        }

        return normalizedParameters.ToArray();
    }

    private Array CreateParamsParameter(Type elementType, object?[]? parameters, int startIndex)
    {
        object?[] paramsArray = parameters?[startIndex..] ?? Array.Empty<object?>();
        Array typedParamsArray = Array.CreateInstance(elementType, paramsArray.Length);

        try
        {
            for (int i = 0; i < paramsArray.Length; i++)
            {
                typedParamsArray.SetValue(paramsArray[i], i);
            }
        }
        catch (InvalidCastException exception)
        {
            throw new ArgumentException($"The provided arguments contain at least one argument that is not compatible with the parameter 'params {elementType}[]'.", exception);
        }
        

        return typedParamsArray;
    }

    /// <summary>
    /// Invokes the underlying method represented by this class with the provided generic types and parameters.
    /// </summary>
    /// <param name="objectInstance">The object instance on which the invoke process will take place.</param>
    /// <param name="genericTypes">Any generic types used in the invoke process.</param>
    /// <param name="parameters">Any parameters required by the invoke process.</param>
    /// <returns>An object resulting form the invoke process. A null value is returned if nothing was intended to be returned.</returns>
    public override object? Invoke(object objectInstance, Type[]? genericTypes, object?[]? parameters)
    {
        MethodInfo invokeMethodInfo = TargetMemberInfo;
        if (genericTypes is { Length: > 0 })
        {
            invokeMethodInfo = TargetMemberInfo.MakeGenericMethod(genericTypes);
        }

        object?[] normalizedParameters = NormalizeParameters(parameters);
        return invokeMethodInfo.Invoke(objectInstance, normalizedParameters);
    }
}