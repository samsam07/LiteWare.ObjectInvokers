using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteWare.ObjectInvokers.Attributes;
using LiteWare.ObjectInvokers.Extensions;
using NUnit.Framework;

#pragma warning disable CS0649
#pragma warning disable CS0169
#pragma warning disable IDE0044
#pragma warning disable CA1822

namespace LiteWare.ObjectInvokers.UnitTests.ExtensionsTests;

[TestFixture]
public class ObjectContractExtensionTest
{
    #region Stubs

    private const string CustomObjectMemberName = "CustomName";

    private class StubClassBase
    {
        public bool UnInvokableField;

        [InvokableMember]
        public static int BaseProperty { get; set; }
    }

    private class StubClass : StubClassBase
    {
        public const string PrivateFieldName = nameof(_privateField);

        [InvokableMember]
        private bool _privateField;

        [InvokableMember(CustomObjectMemberName)]
        public static int Property { get; set; }

        [InvokableMember]
        public string Method() => nameof(Method);

        public bool UnInvokableMethod() => false;
    }

    #endregion

    [TestCase(nameof(StubClass.PrivateFieldName), TestName = "ToObjectMember_Should_CreateObjectField_When_MemberIsAField", ExpectedResult = typeof(ObjectField))]
    [TestCase(nameof(StubClass.Property), TestName = "ToObjectMember_Should_CreateObjectProperty_When_MemberIsAProperty", ExpectedResult = typeof(ObjectProperty))]
    [TestCase(nameof(StubClass.Method), TestName = "ToObjectMember_Should_CreateObjectMethod_When_MemberIsAMethod", ExpectedResult = typeof(ObjectMethod))]
    public Type ToObjectMember_Should_Create(string stubMemberName)
    {
        MemberInfo memberInfo = typeof(StubClass).GetMember(stubMemberName).First();

        IObjectMember objectMember = memberInfo.ToObjectMember();

        return objectMember.GetType();
    }

    [Test]
    public void ToObjectMember_Should_ThrowNotSupportedException_When_MemberCannotBeConvertedToObjectMember()
    {
        MemberInfo memberInfo = typeof(StubClass).GetConstructors().First();

        NotSupportedException exception = Assert.Throws<NotSupportedException>(() => memberInfo.ToObjectMember())!;
        Assert.That(exception.Message, Is.EqualTo("Member of the type 'RuntimeConstructorInfo' is not supported."));
    }

    [Test]
    public void FindInvokableMembers_Should_ReturnAllMembersQualifiedByInvokableMemberAttribute()
    {
        IEnumerable<IObjectMember> objectMembers = typeof(StubClass).FindInvokableMembers().ToList();

        Assert.That(objectMembers, Has.Exactly(4).Items, "Object member count");
        Assert.That(objectMembers, Has.One.With.Property(nameof(IObjectMember.PreferredName)).EqualTo(nameof(StubClassBase.BaseProperty)), "Found member in base class");
        Assert.That(objectMembers, Has.One.With.Property(nameof(IObjectMember.PreferredName)).EqualTo(StubClass.PrivateFieldName), "Found private field member");
        Assert.That(objectMembers, Has.One.With.Property(nameof(IObjectMember.PreferredName)).EqualTo(CustomObjectMemberName), "Found member with custom name");
        Assert.That(objectMembers, Has.One.With.Property(nameof(IObjectMember.PreferredName)).EqualTo(nameof(StubClass.Method)), "Found normal member");
    }

    [Test]
    public void FindInvokableMembers_Should_ReturnMemberWithCustomName_When_QualifiedByInvokableMemberAttributeWithCustomName()
    {
        IEnumerable<IObjectMember> objectMembers = typeof(StubClass)
            .FindInvokableMembers()
            .Where(m => m.PreferredName == CustomObjectMemberName);

        Assert.That(objectMembers, Has.One.Items);
    }
}