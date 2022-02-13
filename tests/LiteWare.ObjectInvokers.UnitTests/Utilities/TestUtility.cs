using NUnit.Framework;
using System;
using System.Text.RegularExpressions;

namespace LiteWare.ObjectInvokers.UnitTests.Utilities;

public static class TestUtility
{
    /// <summary>
    /// Converts a string representing a parameter signature to an instance of <see cref="MemberParameter"/>.
    /// </summary>
    /// <param name="parameterLiteral">A string representing a parameter signature.</param>
    /// <param name="parameterPosition">The position of the parameter.</param>
    /// <returns>An instance of <see cref="MemberParameter"/>.</returns>
    /// <remarks>
    /// <paramref name="parameterLiteral"/> is in the form (EBNF):
    ///     parameterLiteralType    =   ? any valid .NET type name ?            (* Represents a parameter type, i.e: System.String[] *)
    ///                             |   "?"                                     (* Represents a generic parameter type *)
    ///                             ;
    ///     parameterLiteral        =   parameterLiteralType                    (* Represents a parameter with the type defined by "type name" *)
    ///                             |   "[" , parameterLiteralType , "]"        (* Represents an optional parameter *)
    ///                             |   "@" , parameterLiteralType              (* Represents a parameter qualified by the params keyword *)
    ///                             ;
    /// </remarks>
    /// <example>
    /// Examples of <paramref name="parameterLiteral"/>:
    ///     "System.Int32"          ->      A parameter of the Int32 type.
    ///     "[System.Int32]"        ->      An optional parameter of the Int32 type.
    ///     "@System.String[]"      ->      An params parameter of the String[] type.
    ///     "?"                     ->      A generic parameter.
    ///     "[?]"                   ->      An optional generic parameter.
    ///     "@?"                    ->      A params generic parameter.
    /// </example>
    public static MemberParameter ConvertLiteralToParameter(string parameterLiteral, int parameterPosition)
    {
        parameterLiteral = parameterLiteral.Trim();

        bool isOptional = parameterLiteral.StartsWith('[') && parameterLiteral.EndsWith(']');
        bool isParams = parameterLiteral.StartsWith('@');

        string typeName = parameterLiteral.Replace("@", string.Empty);
        if (isOptional)
        {
            typeName = typeName[1..^1];
        }

        bool isGenericType = (typeName == "?");

        Type parameterType = (isGenericType) ? Type.MakeGenericMethodParameter(parameterPosition) : Type.GetType(typeName, true)!;
        return new MemberParameter(parameterType, isOptional, isParams);
    }

    public static void AssertThatSignatureIsMatching(MemberSignature signature, string expectedMemberName, int expectedGenericTypeCount, params string[] expectedParameterLiterals)
    {
        Assert.That(signature.Name, Is.EqualTo(expectedMemberName), "Signature name");
        Assert.That(signature.GenericTypeCount, Is.EqualTo(expectedGenericTypeCount), "Signature generic type count");
        Assert.That(signature.Parameters.Length, Is.EqualTo(expectedParameterLiterals.Length), "Signature parameter count");

        for (int i = 0; i < expectedParameterLiterals.Length; i++)
        {
            string expectedParameterLiteral = expectedParameterLiterals[i];
            MemberParameter expectedParameter = ConvertLiteralToParameter(expectedParameterLiteral, i);
            MemberParameter parameter = signature.Parameters[i];

            Assert.That(parameter.IsGeneric, Is.EqualTo(expectedParameter.IsGeneric), "Signature parameter is generic");
            Assert.That(parameter.IsOptional, Is.EqualTo(expectedParameter.IsOptional), "Signature parameter is optional");
            Assert.That(parameter.IsParamArray, Is.EqualTo(expectedParameter.IsParamArray), "Signature parameter is param array");

            if (!expectedParameter.IsGeneric)
            {
                Assert.That(parameter.ParameterType, Is.EqualTo(expectedParameter.ParameterType), "Signature parameter type");
            }
        }
    }
}