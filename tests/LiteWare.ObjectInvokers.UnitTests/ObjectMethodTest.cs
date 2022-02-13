using LiteWare.ObjectInvokers.UnitTests.Utilities;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

#pragma warning disable CA1822

namespace LiteWare.ObjectInvokers.UnitTests;

[TestFixture]
public class ObjectMethodTest
{
    #region Stubs

    private const int StaticIntFuncValue = 100;
    private const int ProcDefaultIntValue = 10;

    private class ClassA { }

    private class ClassB : ClassA { }

    private class ClassC : ClassB { }

    private class StubClass
    {
        public static int StaticIntFunc() => StaticIntFuncValue;

        public int Count;

        public void Proc() => Count = -1;

        public string StringFunc_ClassA(ClassA classA) => classA.GetType().Name;

        public void Proc_Int_Str(int count, string multiplier) => Count = count * Convert.ToInt32(multiplier);
        
        public int IntFunc_IntDef(int count = ProcDefaultIntValue) => count;

        public int IntFunc_IntParams(params int[] value) => value?.Sum() ?? 0;

        public void Proc_IntRef(ref int i) => i += 10;

        public void Proc_IntOut(out int i) => i = -1;

        public T? TFunc__T<T>() => default;

        public string StringFunc__TClassB__T<T>(T t) where T : ClassB => t.GetType().Name;
    }

    #endregion

    [TestCase(nameof(StubClass.StaticIntFunc), 0, TestName = "Signature_Should_NotContainParameter_When_StaticMethodIsParameterLess")]
    [TestCase(nameof(StubClass.Proc), 0, TestName = "Signature_Should_NotContainParameter_When_MethodIsParameterLess")]
    [TestCase(nameof(StubClass.StringFunc_ClassA), 0, "LiteWare.ObjectInvokers.UnitTests.ObjectMethodTest+ClassA", TestName = "Signature_Should_ContainOneParameter_When_MethodHasOneParameter")]
    [TestCase(nameof(StubClass.Proc_Int_Str), 0, "System.Int32", "System.String", TestName = "Signature_Should_ContainTwoParameters_When_MethodHasTwoParameters")]
    [TestCase(nameof(StubClass.IntFunc_IntDef), 0, "[System.Int32]", TestName = "Signature_Should_ContainOneDefaultParameter_When_MethodHasOneDefaultParameter")]
    [TestCase(nameof(StubClass.IntFunc_IntParams), 0, "@System.Int32[]", TestName = "Signature_Should_ContainOneParamsParameter_When_MethodHasOneParamsParameter")]
    [TestCase(nameof(StubClass.TFunc__T), 1, TestName = "Signature_Should_HaveOneGenericTypeCount_When_MethodHasOneGenericType")]
    [TestCase(nameof(StubClass.StringFunc__TClassB__T), 1, "?", TestName = "Signature_Should_HaveOneGenericTypeCount_And_OneGenericParameter_When_MethodHasOneGenericParameter")]
    public void Signature_Should_BeCorrectlyBuild(string stubMethodName, int expectedGenericTypeCount, params string[] expectedParameterLiterals)
    {
        MethodInfo methodInfo = typeof(StubClass).GetMethod(stubMethodName)!;

        ObjectMethod objectMethod = new(methodInfo);

        TestUtility.AssertThatSignatureIsMatching(objectMethod.Signature, methodInfo.Name, expectedGenericTypeCount, expectedParameterLiterals);
    }

    [TestCase(nameof(StubClass.Proc_IntRef), TestName = "Signature_Should_ThrowNotSupportedException_When_MethodHasRefParameter")]
    [TestCase(nameof(StubClass.Proc_IntOut), TestName = "Signature_Should_ThrowNotSupportedException_When_MethodHasOutParameter")]
    public void Signature_Should_ThrowNotSupportedException(string stubMethodName)
    {
        MethodInfo methodInfo = typeof(StubClass).GetMethod(stubMethodName)!;

        NotSupportedException exception = Assert.Throws<NotSupportedException>(() => new ObjectMethod(methodInfo))!;
        Assert.That(exception.Message, Is.EqualTo("Parameters passed by ref/out keyword are not supported."));
    }

    [TestCase(nameof(StubClass.StaticIntFunc), null, null, TestName = "Invoke_Should_ReturnCorrectValue_When_MethodIsStaticFunction", ExpectedResult = StaticIntFuncValue)]
    [TestCase(nameof(StubClass.Proc), null, null, TestName = "Invoke_Should_ReturnNull_When_MethodIsProcedure", ExpectedResult = null)]
    [TestCase(nameof(StubClass.IntFunc_IntDef), null, null, TestName = "Invoke_Should_UseDefaultValue_When_MethodHasDefaultParameter_And_NoParameterIsProvided", ExpectedResult = ProcDefaultIntValue)]
    [TestCase(nameof(StubClass.IntFunc_IntDef), null, new object[] { }, TestName = "Invoke_Should_UseDefaultValue_When_MethodHasDefaultParameter_And_EmptyParameterIsProvided", ExpectedResult = ProcDefaultIntValue)]
    [TestCase(nameof(StubClass.IntFunc_IntDef), null, new object[] { 123 }, TestName = "Invoke_Should_UseProvidedParameter_When_MethodHasDefaultParameter_And_ParameterIsProvided", ExpectedResult = 123)]
    [TestCase(nameof(StubClass.IntFunc_IntParams), null, null, TestName = "Invoke_Should_UseEmptyParamsArray_When_MethodHasParamsParameter_And_NoParameterIsProvided", ExpectedResult = 0)]
    [TestCase(nameof(StubClass.IntFunc_IntParams), null, new object[] {}, TestName = "Invoke_Should_UseEmptyParamsArray_When_MethodHasParamsParameter_And_EmptyParameterIsProvided", ExpectedResult = 0)]
    [TestCase(nameof(StubClass.IntFunc_IntParams), null, new object[] { '0', 1, 2 }, TestName = "Invoke_Should_UseProvidedParameters_When_MethodHasParamsParameter_And_ParametersAreProvided", ExpectedResult = 51)]
    [TestCase(nameof(StubClass.TFunc__T), new[] { typeof(int) }, null, TestName = "Invoke_Should_AcceptGenericType_When_MethodIsGeneric_And_GenericTypeIsProvided", ExpectedResult = 0)]
    public object? Invoke_Should_ReturnCorrectValue(string stubMethodName, Type[]? genericTypes, object?[]? parameters)
    {
        StubClass stubClass = new();
        MethodInfo methodInfo = typeof(StubClass).GetMethod(stubMethodName)!;
        ObjectMethod objectMethod = new(methodInfo);
        
        return objectMethod.Invoke(stubClass, genericTypes, parameters);
    }

    [Test]
    public void Invoke_Should_AcceptParameter_When_ValidParameterIsProvided()
    {
        StubClass stubClass = new();
        MethodInfo methodInfo = typeof(StubClass).GetMethod(nameof(StubClass.StringFunc_ClassA))!;
        ObjectMethod objectMethod = new(methodInfo);

        object? result = objectMethod.Invoke(stubClass, null, new object[] { new ClassA() });

        Assert.That(result, Is.EqualTo(nameof(ClassA)));
    }

    [Test]
    public void Invoke_Should_AcceptParameter_When_DerivedObjectParameterIsProvided()
    {
        StubClass stubClass = new();
        MethodInfo methodInfo = typeof(StubClass).GetMethod(nameof(StubClass.StringFunc_ClassA))!;
        ObjectMethod objectMethod = new(methodInfo);

        object? result = objectMethod.Invoke(stubClass, null, new object[] { new ClassB() });

        Assert.That(result, Is.EqualTo(nameof(ClassB)));
    }

    [Test]
    public void Invoke_Should_AcceptDerivedGenericType_When_MethodIsGeneric_And_DerivedGenericTypeIsProvided()
    {
        StubClass stubClass = new();
        MethodInfo methodInfo = typeof(StubClass).GetMethod(nameof(StubClass.StringFunc__TClassB__T))!;
        ObjectMethod objectMethod = new(methodInfo);

        object? result = objectMethod.Invoke(stubClass, new[] { typeof(ClassB) }, new object[] { new ClassC() });

        Assert.That(result, Is.EqualTo(nameof(ClassC)));
    }

    [TestCase(nameof(StubClass.Proc), new[] { typeof(int) }, null, TestName = "Invoke_Should_ThrowInvalidOperationException_When_GenericTypesAreProvided_And_MethodIsNotGeneric")]
    [TestCase(nameof(StubClass.TFunc__T), null, null, TestName = "Invoke_Should_ThrowInvalidOperationException_When_GenericTypesAreNotProvided_And_MethodIsGeneric")]
    [TestCase(nameof(StubClass.TFunc__T), new Type[] { }, null, TestName = "Invoke_Should_ThrowInvalidOperationException_When_EmptyGenericTypesAreProvided_And_MethodIsGeneric")]
    public void Invoke_Should_ThrowInvalidOperationException(string stubMethodName, Type[]? genericTypes, object?[]? parameters)
    {
        StubClass stubClass = new();
        MethodInfo methodInfo = typeof(StubClass).GetMethod(stubMethodName)!;
        ObjectMethod objectMethod = new(methodInfo);

        Assert.Throws<InvalidOperationException>(() => objectMethod.Invoke(stubClass, genericTypes, parameters));
    }

    [TestCase(nameof(StubClass.StringFunc__TClassB__T), new[] { typeof(int) }, new object[] { 1 }, TestName = "Invoke_Should_ThrowArgumentException_When_InvalidGenericTypeIsProvided_And_MethodIsGeneric")]
    [TestCase(nameof(StubClass.StringFunc_ClassA), null, new object[] { 1 }, TestName = "Invoke_Should_ThrowArgumentException_When_ProvidedParameterIsInvalid")]
    [TestCase(nameof(StubClass.TFunc__T), new[] { typeof(int), typeof(string) }, null, TestName = "Invoke_Should_ThrowArgumentException_When_TwoGenericTypesAreProvided_And_GenericMethodExpectsOneGenericType")]
    public void Invoke_Should_ThrowArgumentException(string stubMethodName, Type[]? genericTypes, object?[]? parameters)
    {
        StubClass stubClass = new();
        MethodInfo methodInfo = typeof(StubClass).GetMethod(stubMethodName)!;
        ObjectMethod objectMethod = new(methodInfo);

        Assert.Throws<ArgumentException>(() => objectMethod.Invoke(stubClass, genericTypes, parameters));
    }

    [Test]
    public void Invoke_Should_ThrowArgumentException_When_IncompatibleArgumentsAreProvided_And_MethodHasParamsParameter()
    {
        StubClass stubClass = new();
        MethodInfo methodInfo = typeof(StubClass).GetMethod(nameof(StubClass.IntFunc_IntParams))!;
        ObjectMethod objectMethod = new(methodInfo);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => objectMethod.Invoke(stubClass, 1, "2"))!;
        Assert.That(exception.Message, Is.EqualTo("The provided arguments contain at least one argument that is not compatible with the parameter 'params System.Int32[]'."));
    }

    [TestCase(nameof(StubClass.StringFunc_ClassA), null, null, TestName = "Invoke_Should_ThrowTargetParameterCountException_When_NoParametersAreProvided_And_MethodExpectsParameters")]
    [TestCase(nameof(StubClass.StringFunc_ClassA), null, new object[] { }, TestName = "Invoke_Should_ThrowTargetParameterCountException_When_EmptyParametersAreProvided_And_MethodExpectsParameters")]
    [TestCase(nameof(StubClass.Proc_Int_Str), null, new object[] { 1, "2", true }, TestName = "Invoke_Should_ThrowTargetParameterCountException_When_ExcessiveParametersAreProvided")]
    public void Invoke_Should_ThrowTargetParameterCountException(string stubMethodName, Type[]? genericTypes, object?[]? parameters)
    {
        StubClass stubClass = new();
        MethodInfo methodInfo = typeof(StubClass).GetMethod(stubMethodName)!;
        ObjectMethod objectMethod = new(methodInfo);

        Assert.Throws<TargetParameterCountException>(() => objectMethod.Invoke(stubClass, genericTypes, parameters));
    }
}