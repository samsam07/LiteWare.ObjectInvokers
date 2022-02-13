using System;
using System.Linq;
using System.Reflection;
using LiteWare.ObjectInvokers.UnitTests.Utilities;
using NUnit.Framework;

namespace LiteWare.ObjectInvokers.UnitTests;

[TestFixture]
public class ObjectFieldTest
{
    #region Stubs

    private const int ConstantFieldValue = 100;
    private const string StaticFieldValue = "StaticFieldValue";
    private const int ReadonlyFieldValue = 1;
    private const bool NormalFieldValue = true;

    private class StubClass
    {
        public const int ConstantField = ConstantFieldValue;

        public static string StaticField = StaticFieldValue;

        public readonly int ReadonlyField = ReadonlyFieldValue;

        public bool NormalField = NormalFieldValue;

        public StubClass()
        {
            StaticField = StaticFieldValue; // Reset value
        }
    }

    #endregion

    [TestCase(nameof(StubClass.ConstantField), TestName = "Signature_Should_NotContainParameter_When_FieldIsConstant")]
    [TestCase(nameof(StubClass.StaticField), "[System.String]", TestName = "Signature_Should_ContainParameter_When_FieldIsStatic")]
    [TestCase(nameof(StubClass.ReadonlyField), TestName = "Signature_Should_NotContainParameter_When_FieldIsReadonly")]
    [TestCase(nameof(StubClass.NormalField), "[System.Boolean]", TestName = "Signature_Should_ContainParameter_When_FieldIsNormal")]
    public void Signature_Should_BeCorrectlyBuild(string stubFieldName, params string[] expectedParameterLiterals)
    {
        FieldInfo fieldInfo = typeof(StubClass).GetField(stubFieldName)!;

        ObjectField objectField = new(fieldInfo);

        TestUtility.AssertThatSignatureIsMatching(objectField.Signature, fieldInfo.Name, 0, expectedParameterLiterals);
    }

    [TestCase(nameof(StubClass.ConstantField), TestName = "CanWrite_Should_BeFalse_When_FieldIsReadonly", ExpectedResult = false)]
    [TestCase(nameof(StubClass.ReadonlyField), TestName = "CanWrite_Should_BeFalse_When_FieldIsConstant", ExpectedResult = false)]
    [TestCase(nameof(StubClass.NormalField), TestName = "CanWrite_Should_BeTrue_When_FieldIsNormal", ExpectedResult = true)]
    public bool CanWrite_Should_BeCorrectlySet(string stubFieldName)
    {
        FieldInfo fieldInfo = typeof(StubClass).GetField(stubFieldName)!;

        ObjectField objectField = new(fieldInfo);

        return objectField.CanWrite;
    }

    [TestCase(nameof(StubClass.ConstantField), TestName = "GetValue_Should_ReturnCorrectValue_When_FieldIsConstant", ExpectedResult = ConstantFieldValue)]
    [TestCase(nameof(StubClass.StaticField), TestName = "GetValue_Should_ReturnCorrectValue_When_FieldIsStatic", ExpectedResult = StaticFieldValue)]
    public object? GetValue_Should_ReturnCorrectValue(string stubFieldName)
    {
        StubClass stubClass = new();
        FieldInfo fieldInfo = typeof(StubClass).GetField(stubFieldName)!;
        ObjectField objectField = new(fieldInfo);

        return objectField.GetValue(stubClass);
    }

    [TestCase(nameof(StubClass.StaticField), "NewValue", TestName = "SetValue_Should_AssignCorrectValue_When_FieldIsStatic")]
    [TestCase(nameof(StubClass.NormalField), false, TestName = "SetValue_Should_AssignCorrectValue_When_FieldIsNormal")]
    public void SetValue_Should_AssignCorrectValue(string stubFieldName, object? value)
    {
        StubClass stubClass = new();
        FieldInfo fieldInfo = typeof(StubClass).GetField(stubFieldName)!;
        ObjectField objectField = new(fieldInfo);

        objectField.SetValue(stubClass, value);

        object? actualValue = fieldInfo.GetValue(stubClass);
        Assert.That(actualValue, Is.EqualTo(value));
    }

    [TestCase(nameof(StubClass.ConstantField), 0, TestName = "SetValue_Should_ThrowInvalidOperationException_When_FieldIsConstant")]
    [TestCase(nameof(StubClass.ReadonlyField), 0, TestName = "SetValue_Should_ThrowInvalidOperationException_When_FieldIsReadonly")]
    public void SetValue_Should_ThrowInvalidOperationException_When_FieldCannotBeWrittenTo(string stubFieldName, object? value)
    {
        StubClass stubClass = new();
        FieldInfo fieldInfo = typeof(StubClass).GetField(stubFieldName)!;
        ObjectField objectField = new(fieldInfo);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => objectField.SetValue(stubClass, value))!;
        Assert.That(exception.Message, Is.EqualTo("Field is read only."));
    }

    [Test]
    public void SetValue_Should_ThrowArgumentException_When_ValueToSetIsNotCompatibleWithFieldType()
    {
        const string invalidValue = "Test";

        StubClass stubClass = new();
        FieldInfo fieldInfo = typeof(StubClass).GetField(nameof(StubClass.NormalField))!;
        ObjectField objectField = new(fieldInfo);

        Assert.Throws<ArgumentException>(() => objectField.SetValue(stubClass, invalidValue));
    }

    [TestCase(nameof(StubClass.NormalField), TestName = "Invoke_Should_ReturnFieldValue_When_CalledWithNoParameterValue", ExpectedResult = NormalFieldValue)]
    [TestCase(nameof(StubClass.StaticField), new object?[] { }, TestName = "Invoke_Should_ReturnFieldValue_When_CalledWithEmptyParameterValue", ExpectedResult = StaticFieldValue)]
    public object? Invoke_Should_ReturnFieldValue(string stubFieldName, params object?[] parameters)
    {
        StubClass stubClass = new();
        FieldInfo fieldInfo = typeof(StubClass).GetField(stubFieldName)!;
        ObjectField objectField = new(fieldInfo);
        
        return objectField.Invoke(stubClass, parameters);
    }

    [Test]
    public void Invoke_Should_AssignFieldValue_When_CalledWithOneParameter()
    {
        const string newValue = "NewValue";

        StubClass stubClass = new();
        FieldInfo fieldInfo = typeof(StubClass).GetField(nameof(StubClass.StaticField))!;
        ObjectField objectField = new(fieldInfo);

        objectField.Invoke(stubClass, newValue);

        object? actualValue = fieldInfo.GetValue(stubClass);
        Assert.That(actualValue, Is.EqualTo(newValue));
    }

    [Test]
    public void Invoke_Should_ReturnNull_When_CalledWithOneParameter()
    {
        const string newValue = "NewValue";

        StubClass stubClass = new();
        FieldInfo fieldInfo = typeof(StubClass).GetField(nameof(StubClass.StaticField))!;
        ObjectField objectField = new(fieldInfo);

        object? returnValue = objectField.Invoke(stubClass, newValue);

        Assert.That(returnValue, Is.Null);
    }

    [Test]
    public void Invoke_Should_ThrowArgumentException_When_GenericTypesArgumentIsProvided()
    {
        StubClass stubClass = new();
        FieldInfo fieldInfo = typeof(StubClass).GetField(nameof(StubClass.NormalField))!;
        ObjectField objectField = new(fieldInfo);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => objectField.Invoke(stubClass, new[] { typeof(int) }, null))!;
        Assert.That(exception.Message, Is.EqualTo("Field invoke cannot accept generic types."));
    }

    [Test]
    public void Invoke_Should_NotThrowArgumentException_When_GenericTypesArgumentIsEmpty()
    {
        StubClass stubClass = new();
        FieldInfo fieldInfo = typeof(StubClass).GetField(nameof(StubClass.NormalField))!;
        ObjectField objectField = new(fieldInfo);

        Assert.DoesNotThrow(() => objectField.Invoke(stubClass, Array.Empty<Type>(), null));
    }

    [Test]
    public void Invoke_Should_ThrowArgumentException_When_MoreThanOneParameterIsProvided()
    {
        StubClass stubClass = new();
        FieldInfo fieldInfo = typeof(StubClass).GetField(nameof(StubClass.NormalField))!;
        ObjectField objectField = new(fieldInfo);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => objectField.Invoke(stubClass, 1, 2))!;
        Assert.That(exception.Message, Is.EqualTo("Setting the field can accept only 1 parameter."));
    }
}