using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace LiteWare.ObjectInvokers.UnitTests;

[TestFixture]
public class ObjectMemberTest
{
    #region Stubs

    private const string SignatureName = "StubName";
    private static readonly MemberInfo DummyMemberInfo = typeof(ObjectMemberStub).GetMembers().First();

    private class ObjectMemberStub : ObjectMember<MemberInfo>
    {
        public ObjectMemberStub(string? preferredName = null)
            : base(DummyMemberInfo, preferredName) { }

        public override object? Invoke(object objectInstance, Type[]? genericTypes, object?[]? parameters) =>
            null;

        protected override MemberSignature BuildSignature(MemberInfo memberInfo) =>
            new(SignatureName, 0, Array.Empty<MemberParameter>());
    }

    #endregion

    [Test]
    public void PreferredName_ShouldDefaultTo_SignatureName_When_Omitted()
    {
        ObjectMemberStub objectMemberStub = new();

        Assert.That(objectMemberStub.PreferredName, Is.EqualTo(SignatureName));
    }

    [Test]
    public void PreferredName_ShouldTakePreferenceOver_SignatureName_When_Provided()
    {
        const string preferredName = "PreferredName";

        ObjectMemberStub objectMemberStub = new(preferredName);

        Assert.That(objectMemberStub.PreferredName, Is.EqualTo(preferredName));
    }
}