#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LiteWare.ObjectInvokers.UnitTests")]
#endif
namespace LiteWare.ObjectInvokers.Utilities;

internal static class SignatureDeviancyCalculatorUtility
{
    public const int OptionalParameterScore = 1000;
    public const int TypeConversionScore = 1;

    public static int CalculateDeviancyScore(IObjectMember objectMember, string memberName, int genericTypeCount, object?[]? parameters)
    {
        if (memberName != objectMember.PreferredName || genericTypeCount != objectMember.Signature.GenericTypeCount)
        {
            return MemberSignature.NoMatchScore;
        }

        int providedParameterCount = parameters?.Length ?? 0;
        if (providedParameterCount > objectMember.Signature.Parameters.Length)
        {
            return MemberSignature.NoMatchScore;
        }
        
        return CalculateParametersDeviancyScore(objectMember.Signature.Parameters, parameters);
    }

    public static int CalculateParametersDeviancyScore(MemberParameter[] signatureParameters, object?[]? providedParameters)
    {
        int deviancyScore = 0;

        int providedParameterCount = providedParameters?.Length ?? 0;
        for (int i = 0; i < signatureParameters.Length; i++)
        {
            MemberParameter signatureParameter = signatureParameters[i];

            bool parameterProvided = (i < providedParameterCount);
            if (parameterProvided)
            {
                object? providedParameter = providedParameters![i];
                ValidateProvidedParameter(signatureParameter, providedParameter, ref deviancyScore);
            }
            else
            {
                ValidateMissingParameter(signatureParameter, ref deviancyScore);
            }

            if (deviancyScore == MemberSignature.NoMatchScore)
            {
                return MemberSignature.NoMatchScore;
            }
        }

        // Any excess parameters
        if (providedParameterCount > signatureParameters.Length)
        {
            MemberParameter? lastParameter = signatureParameters.LastOrDefault();
            if (lastParameter is null || !lastParameter.IsParamArray)
            {
                return MemberSignature.NoMatchScore;
            }

            int excessiveParameterStartIndex = signatureParameters.Length;
            HandleParamArrayParameters(lastParameter, providedParameters![excessiveParameterStartIndex..], ref deviancyScore);
        }

        return deviancyScore;
    }

    private static void HandleParamArrayParameters(MemberParameter signatureParameter, object?[] parameters, ref int deviancyScore)
    {
        foreach (object? parameter in parameters)
        {
            ValidateProvidedParameter(signatureParameter, parameter, ref deviancyScore);
            if (deviancyScore == MemberSignature.NoMatchScore)
            {
                return;
            }
        }
    }

    private static void ValidateProvidedParameter(MemberParameter signatureParameter, object? providedParameter, ref int deviancyScore)
    {
        Type signatureParameterType = signatureParameter.ParameterType;
        if (signatureParameter.IsGeneric)
        {
            signatureParameterType = signatureParameter.ParameterType.BaseType ?? typeof(object);
        }

        if (providedParameter is null)
        {
            if (TypeUtility.IsNullable(signatureParameterType))
            {
                deviancyScore += TypeConversionScore;
            }
            else
            {
                deviancyScore = MemberSignature.NoMatchScore;
            }

            return;
        }

        Type providedParameterType = providedParameter.GetType();
        if (providedParameterType == signatureParameterType)
        {
            return;
        }

        bool canImplicitlyConvert = TypeUtility.CanImplicitlyConvert(providedParameterType, signatureParameterType);
        if (!canImplicitlyConvert)
        {
            deviancyScore = MemberSignature.NoMatchScore;
        }
        else if (!signatureParameter.IsGeneric)
        {
            deviancyScore += TypeConversionScore;
        }
    }

    private static void ValidateMissingParameter(MemberParameter signatureParameter, ref int deviancyScore)
    {
        if (signatureParameter.IsParamArray)
        {
            return;
        }

        if (signatureParameter.IsOptional)
        {
            deviancyScore += OptionalParameterScore;
        }
        else
        {
            deviancyScore = MemberSignature.NoMatchScore;
        }
    }
}