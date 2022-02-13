using System;
using System.Linq;
using System.Reflection;
using LiteWare.ObjectInvokers.UnitTests.Utilities;
using NUnit.Framework;

namespace LiteWare.ObjectInvokers.UnitTests;

[TestFixture]
public class ObjectPropertyTest
{
    #region Stubs

    private const int StaticPropValue = 10;
    private const int NormalPropValue = 100;
    private const string GetterOnlyPropValue = "GetterOnly";
    private const string SetterOnlyPropValue = "SetterOnly";

    private class StubClass
    {
        public static int StaticProp { get; set; }

        public string SetterOnlyPropBackingField = SetterOnlyPropValue;

        public int NormalProp { get; set; } = NormalPropValue;

        public string GetterOnlyProp
        {
            get => GetterOnlyPropValue;
        }
        
        public string SetterOnlyProp
        {
            set => SetterOnlyPropBackingField = value;
        }

        public StubClass()
        {
            StaticProp = StaticPropValue;
        }
    }

    #endregion

    [TestCase(nameof(StubClass.StaticProp), "[System.Int32]", TestName = "Signature_Should_ContainParameter_When_PropertyIsStatic")]
    [TestCase(nameof(StubClass.NormalProp), "[System.Int32]", TestName = "Signature_Should_ContainParameter_When_PropertyIsNormal")]
    [TestCase(nameof(StubClass.GetterOnlyProp), TestName = "Signature_Should_NotContainParameter_When_PropertyIsGetterOnly")]
    [TestCase(nameof(StubClass.SetterOnlyProp), "System.String", TestName = "Signature_Should_ContainParameter_When_PropertyIsSetterOnly")]
    public void Signature_Should_BeCorrectlyBuild(string stubPropertyName, params string[] expectedParameterLiterals)
    {
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(stubPropertyName)!;

        ObjectProperty objectProperty = new(propertyInfo);

        TestUtility.AssertThatSignatureIsMatching(objectProperty.Signature, propertyInfo.Name, 0, expectedParameterLiterals);
    }

    [TestCase(nameof(StubClass.NormalProp), TestName = "CanRead_Should_BeTrue_When_PropertyIsNormal", ExpectedResult = true)]
    [TestCase(nameof(StubClass.GetterOnlyProp), TestName = "CanRead_Should_BeTrue_When_PropertyIsGetterOnly", ExpectedResult = true)]
    [TestCase(nameof(StubClass.SetterOnlyProp), TestName = "CanRead_Should_BeFalse_When_PropertyIsSetterOnly", ExpectedResult = false)]
    public bool CanRead_Should_BeCorrectlySet(string stubPropertyName)
    {
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(stubPropertyName)!;

        ObjectProperty objectProperty = new(propertyInfo);

        return objectProperty.CanRead;
    }

    [TestCase(nameof(StubClass.NormalProp), TestName = "CanWrite_Should_BeTrue_When_PropertyIsNormal", ExpectedResult = true)]
    [TestCase(nameof(StubClass.GetterOnlyProp), TestName = "CanWrite_Should_BeFalse_When_PropertyIsGetterOnly", ExpectedResult = false)]
    [TestCase(nameof(StubClass.SetterOnlyProp), TestName = "CanWrite_Should_BeTrue_When_PropertyIsSetterOnly", ExpectedResult = true)]
    public bool CanWrite_Should_BeCorrectlySet(string stubPropertyName)
    {
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(stubPropertyName)!;

        ObjectProperty objectProperty = new(propertyInfo);

        return objectProperty.CanWrite;
    }

    [TestCase(nameof(StubClass.NormalProp), TestName = "GetValue_Should_ReturnCorrectValue_When_PropertyIsNormal", ExpectedResult = NormalPropValue)]
    [TestCase(nameof(StubClass.GetterOnlyProp), TestName = "GetValue_Should_ReturnCorrectValue_When_PropertyIsGetterOnly", ExpectedResult = GetterOnlyPropValue)]
    public object? GetValue_Should_ReturnCorrectValue(string stubPropertyName)
    {
        StubClass stubClass = new();
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(stubPropertyName)!;
        ObjectProperty objectProperty = new(propertyInfo);

        return objectProperty.GetValue(stubClass);
    }

    [Test]
    public void GetValue_Should_ThrowInvalidOperationException_When_PropertyCannotBeRead()
    {
        StubClass stubClass = new();
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(nameof(StubClass.SetterOnlyProp))!;
        ObjectProperty objectProperty = new(propertyInfo);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => objectProperty.GetValue(stubClass))!;
        Assert.That(exception.Message, Is.EqualTo("Property is write only."));
    }

    [Test]
    public void SetValue_Should_AssignCorrectValue_When_PropertyIsNormal()
    {
        const int newValue = 1;

        StubClass stubClass = new();
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(nameof(stubClass.NormalProp))!;
        ObjectProperty objectProperty = new(propertyInfo);

        objectProperty.SetValue(stubClass, newValue);

        object? actualValue = objectProperty.GetValue(stubClass);
        Assert.That(actualValue, Is.EqualTo(newValue));
    }

    [Test]
    public void SetValue_Should_AssignCorrectValue_When_PropertyIsSetterOnly()
    {
        const string newValue = "NewValue";

        StubClass stubClass = new();
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(nameof(StubClass.SetterOnlyProp))!;
        ObjectProperty objectProperty = new(propertyInfo);

        objectProperty.SetValue(stubClass, newValue);

        object actualValue = stubClass.SetterOnlyPropBackingField;
        Assert.That(actualValue, Is.EqualTo(newValue));
    }

    [Test]
    public void SetValue_Should_ThrowInvalidOperationException_When_PropertyCannotBeWrittenTo()
    {
        const string newValue = "NewValue";

        StubClass stubClass = new();
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(nameof(StubClass.GetterOnlyProp))!;
        ObjectProperty objectProperty = new(propertyInfo);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => objectProperty.SetValue(stubClass, newValue))!;
        Assert.That(exception.Message, Is.EqualTo("Property is read only."));
    }

    [TestCase(nameof(StubClass.NormalProp), TestName = "Invoke_Should_ReturnPropertyValue_When_CalledWithNoParameterValue", ExpectedResult = NormalPropValue)]
    [TestCase(nameof(StubClass.StaticProp), new object?[] { }, TestName = "Invoke_Should_ReturnPropertyValue_When_CalledWithEmptyParameterValue", ExpectedResult = StaticPropValue)]
    public object? Invoke_Should_ReturnPropertyValue(string stubPropertyName, params object?[] parameters)
    {
        StubClass stubClass = new();
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(stubPropertyName)!;
        ObjectProperty objectProperty = new(propertyInfo);

        return objectProperty.Invoke(stubClass, parameters);
    }

    [Test]
    public void Invoke_Should_AssignPropertyValue_When_CalledWithOneParameter()
    {
        const int newValue = 1;

        StubClass stubClass = new();
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(nameof(StubClass.NormalProp))!;
        ObjectProperty objectProperty = new(propertyInfo);

        objectProperty.Invoke(stubClass, newValue);

        object? actualValue = propertyInfo.GetValue(stubClass);
        Assert.That(actualValue, Is.EqualTo(newValue));
    }

    [Test]
    public void Invoke_Should_ReturnNull_When_CalledWithOneParameter()
    {
        const string newValue = "NewValue";

        StubClass stubClass = new();
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(nameof(StubClass.SetterOnlyProp))!;
        ObjectProperty objectProperty = new(propertyInfo);

        object? returnValue = objectProperty.Invoke(stubClass, newValue);

        Assert.That(returnValue, Is.Null);
    }

    [Test]
    public void Invoke_Should_ThrowArgumentException_When_GenericTypesArgumentIsProvided()
    {
        StubClass stubClass = new();
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(nameof(StubClass.NormalProp))!;
        ObjectProperty objectProperty = new(propertyInfo);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => objectProperty.Invoke(stubClass, new[] { typeof(int) }, null))!;
        Assert.That(exception.Message, Is.EqualTo("Property invoke cannot accept generic types."));
    }

    [Test]
    public void Invoke_Should_NotThrowArgumentException_When_GenericTypesArgumentIsEmpty()
    {
        StubClass stubClass = new();
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(nameof(StubClass.NormalProp))!;
        ObjectProperty objectProperty = new(propertyInfo);

        Assert.DoesNotThrow(() => objectProperty.Invoke(stubClass, Array.Empty<Type>(), null));
    }

    [Test]
    public void Invoke_Should_ThrowArgumentException_When_MoreThanOneParameterIsProvided()
    {
        StubClass stubClass = new();
        PropertyInfo propertyInfo = typeof(StubClass).GetProperty(nameof(StubClass.NormalProp))!;
        ObjectProperty objectProperty = new(propertyInfo);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => objectProperty.Invoke(stubClass, 1, 2))!;
        Assert.That(exception.Message, Is.EqualTo("Setter invoke should contain only 1 parameter."));
    }
}