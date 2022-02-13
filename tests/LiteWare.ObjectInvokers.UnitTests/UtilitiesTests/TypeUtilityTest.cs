using System;
using LiteWare.ObjectInvokers.Utilities;
using NUnit.Framework;

namespace LiteWare.ObjectInvokers.UnitTests.UtilitiesTests;

[TestFixture]
public class TypeUtilityTest
{
    #region Test Objects

    private enum TestEnum { A, B, C }

    private class TestObjectBase { }

    private class TestObject : TestObjectBase { }

    private class ImplicitTestA
    {
        public static implicit operator ImplicitTestA(ImplicitTestB _) => new();

        public static implicit operator ImplicitTestB(ImplicitTestA _) => new();
    }

    private class ImplicitTestB { }

    private class ImplicitTestC { }

    #endregion

    [TestCase(typeof(int), TestName = "IsNullable_Should_ReturnFalse_When_TypeIsInt", ExpectedResult = false)]
    [TestCase(typeof(int?), TestName = "IsNullable_Should_ReturnTrue_When_TypeIsNullableInt", ExpectedResult = true)]
    [TestCase(typeof(string), TestName = "IsNullable_Should_ReturnTrue_When_TypeIsString", ExpectedResult = true)]
    [TestCase(typeof(TestObject), TestName = "IsNullable_Should_ReturnTrue_When_TypeIsTestObject", ExpectedResult = true)]
    public bool IsNullable_Should_ReturnCorrectValue(Type type) =>
        TypeUtility.IsNullable(type);

    [TestCase(typeof(int), typeof(int), TestName = "CanImplicitlyConvert_Should_ReturnTrue_When_TheTypesAreTheSame", ExpectedResult = true)]
    [TestCase(typeof(int), typeof(long), TestName = "CanImplicitlyConvert_Should_ReturnTrue_When_PrimitiveImplicitConversionExist", ExpectedResult = true)]
    [TestCase(typeof(long), typeof(int), TestName = "CanImplicitlyConvert_Should_ReturnFalse_When_PrimitiveImplicitConversionDoesNotExist", ExpectedResult = false)]
    [TestCase(typeof(int), typeof(string), TestName = "CanImplicitlyConvert_Should_ReturnFalse_When_TheTypesAreIncompatible", ExpectedResult = false)]
    [TestCase(typeof(int), typeof(int?), TestName = "CanImplicitlyConvert_Should_ReturnTrue_When_TypeIsConvertedToItsNullableType", ExpectedResult = true)]
    [TestCase(typeof(int?), typeof(int), TestName = "CanImplicitlyConvert_Should_ReturnFalse_When_NullableTypeIsConvertedToItsNonNullableType", ExpectedResult = false)]
    [TestCase(typeof(int), typeof(TestEnum), TestName = "CanImplicitlyConvert_Should_ReturnFalse_When_IntIsConvertedToEnum", ExpectedResult = false)]
    [TestCase(typeof(TestEnum), typeof(int), TestName = "CanImplicitlyConvert_Should_ReturnFalse_When_EnumIsConvertedToInt", ExpectedResult = false)]
    [TestCase(typeof(TestObject), typeof(TestObjectBase), TestName = "CanImplicitlyConvert_Should_ReturnTrue_When_DerivedTypeIsConvertedToBaseType", ExpectedResult = true)]
    [TestCase(typeof(TestObjectBase), typeof(TestObject), TestName = "CanImplicitlyConvert_Should_ReturnFalse_When_BaseTypeIsConvertedToDerivedType", ExpectedResult = false)]
    [TestCase(typeof(ImplicitTestB), typeof(ImplicitTestA), TestName = "CanImplicitlyConvert_Should_ReturnTrue_When_ImplicitConversionExistsInDestinationType", ExpectedResult = true)]
    [TestCase(typeof(ImplicitTestA), typeof(ImplicitTestB), TestName = "CanImplicitlyConvert_Should_ReturnTrue_When_ImplicitConversionExistsInSourceType", ExpectedResult = true)]
    [TestCase(typeof(ImplicitTestB), typeof(ImplicitTestC), TestName = "CanImplicitlyConvert_Should_ReturnFalse_When_ImplicitConversionDoesNotExist", ExpectedResult = false)]
    public bool CanImplicitlyConvert_Should_ReturnCorrectValue(Type typeFrom, Type typeTo) =>
        TypeUtility.CanImplicitlyConvert(typeFrom, typeTo);
}