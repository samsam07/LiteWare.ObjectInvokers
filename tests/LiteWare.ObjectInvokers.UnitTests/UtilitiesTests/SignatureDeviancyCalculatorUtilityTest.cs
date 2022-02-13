using System;
using System.Linq;
using System.Reflection;
using LiteWare.ObjectInvokers.UnitTests.Utilities;
using LiteWare.ObjectInvokers.Utilities;
using Moq;
using NUnit.Framework;

namespace LiteWare.ObjectInvokers.UnitTests.UtilitiesTests;

[TestFixture]
public class SignatureDeviancyCalculatorUtilityTest
{
    private const string SignatureMemberName = "MemberName";
    private const int SignatureGenericTypeCount = 2;

    #region Stubs

    private class StubClass
    {
        public static T GenericMethod<T>(T t) => t;
    }

    #endregion

    private static MemberParameter[] ConvertLiteralsToParameters(string[]? literals)
    {
        if (literals is null)
        {
            return Array.Empty<MemberParameter>();
        }

        MemberParameter[] parameters = new MemberParameter[literals.Length];
        for (int i = 0; i < literals.Length; i++)
        {
            parameters[i] = TestUtility.ConvertLiteralToParameter(literals[i], i);
        }

        return parameters;
    }

    [TestCase("membername", 0, null, TestName = "CalculateDeviancyScore_Should_ReturnNoMatchScore_When_DifferentMemberNameIsProvided")]
    [TestCase(SignatureMemberName, 1, null, TestName = "CalculateDeviancyScore_Should_ReturnNoMatchScore_When_DifferentGenericTypeCountIsProvided")]
    [TestCase(SignatureMemberName, SignatureGenericTypeCount, new object[] { 1, "2" }, TestName = "CalculateDeviancyScore_Should_ReturnNoMatchScore_When_ExcessiveParametersAreProvided")]
    public void CalculateDeviancyScore_Should_ReturnNoMatchScore(string memberName, int genericTypeCount, object?[]? parameters)
    {
        MemberParameter[] signatureParameters = { new(typeof(int)) };
        MemberSignature memberSignature = new(SignatureMemberName, SignatureGenericTypeCount, signatureParameters);
        IObjectMember objectMember = Mock.Of<IObjectMember>
        (
            m =>
                m.PreferredName == SignatureMemberName &&
                m.Signature == memberSignature
        );

        int deviancyScore = SignatureDeviancyCalculatorUtility.CalculateDeviancyScore(objectMember, memberName, genericTypeCount, parameters);

        Assert.That(deviancyScore, Is.EqualTo(MemberSignature.NoMatchScore));
    }

    [TestCase(null, null, ExpectedResult = 0)]
    [TestCase(null, new object[] { }, ExpectedResult = 0)]
    [TestCase(null, new object[] { 1 }, ExpectedResult = MemberSignature.NoMatchScore)]
    [TestCase(new[] { "System.Int32" }, new object[] { 1 }, ExpectedResult = 0)]
    [TestCase(new[] { "System.Int32" }, null, ExpectedResult = MemberSignature.NoMatchScore)]
    [TestCase(new[] { "System.Int32" }, new object[] { }, ExpectedResult = MemberSignature.NoMatchScore)]
    [TestCase(new[] { "System.Int32" }, new object?[] { null }, ExpectedResult = MemberSignature.NoMatchScore)]
    [TestCase(new[] { "System.Int32" }, new object[] { '1' }, ExpectedResult = 1)]
    [TestCase(new[] { "System.Int32" }, new object[] { "1" }, ExpectedResult = MemberSignature.NoMatchScore)]
    [TestCase(new[] { "System.Int32", "System.Int32" }, new object[] { '1', '2' }, ExpectedResult = 2)]
    [TestCase(new[] { "[System.Int32]" }, new object[] { 1 }, ExpectedResult = 0)]
    [TestCase(new[] { "[System.Int32]" }, new object[] { }, ExpectedResult = 1000)]
    [TestCase(new[] { "[System.Int32]" }, null, ExpectedResult = 1000)]
    [TestCase(new[] { "System.Nullable`1[System.Int32]" }, new object?[] { null }, ExpectedResult = 1)]
    [TestCase(new[] { "System.Nullable`1[System.Int32]" }, new object[] { 1 }, ExpectedResult = 1)]
    [TestCase(new[] { "@System.Int32" }, null, ExpectedResult = 0)]
    [TestCase(new[] { "@System.Int32" }, new object[] { }, ExpectedResult = 0)]
    [TestCase(new[] { "@System.Int32" }, new object[] { 1, 2, 3 }, ExpectedResult = 0)]
    [TestCase(new[] { "@System.Int32" }, new object[] { '1', '2', '3' }, ExpectedResult = 3)]
    [TestCase(new[] { "@System.Int32" }, new object[] { 1, 2, "3" }, ExpectedResult = MemberSignature.NoMatchScore)]
    [TestCase(new[] { "@System.Int32" }, new object[] { 1 }, ExpectedResult = 0)]
    public int CalculateParametersDeviancyScore_Should_ReturnCorrectValue(string[]? parameterLiterals, object?[]? providedParameters)
    {
        MemberParameter[] signatureParameters = ConvertLiteralsToParameters(parameterLiterals);

        return SignatureDeviancyCalculatorUtility.CalculateParametersDeviancyScore(signatureParameters, providedParameters);
    }

    [Test]
    public void CalculateParametersDeviancyScore_Should_ReturnCorrectValue_When_ParameterIsGeneric()
    {
        MethodInfo methodInfo = typeof(StubClass).GetMethod(nameof(StubClass.GenericMethod))!;
        ParameterInfo methodParameter = methodInfo.GetParameters().First();
        MemberParameter memberParameter = new(methodParameter.ParameterType);

        int deviancyScore = SignatureDeviancyCalculatorUtility.CalculateParametersDeviancyScore(new [] { memberParameter }, new object[] { 1 });

        Assert.That(deviancyScore, Is.EqualTo(0));
    }
}