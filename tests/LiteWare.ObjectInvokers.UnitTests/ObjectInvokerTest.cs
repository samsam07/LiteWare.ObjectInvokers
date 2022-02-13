using System;
using System.Collections.Generic;
using LiteWare.ObjectInvokers.Exceptions;
using Moq;
using NUnit.Framework;

namespace LiteWare.ObjectInvokers.UnitTests;

[TestFixture]
public class ObjectInvokerTest
{
    private const string DummyMemberName = "DummyMember";
    private readonly object _dummyInstance = new();

    private IObjectMember MockMember(int deviancyScore, object? invokeResult = null) =>
        Mock.Of<IObjectMember>
            (
                m =>
                    m.CalculateSignatureDeviancyScore(DummyMemberName, It.IsAny<int>(), It.IsAny<object?[]?>()) == deviancyScore &&
                    m.Invoke(_dummyInstance, It.IsAny<Type[]?>(), It.IsAny<object?[]?>()) == invokeResult
            );

    [Test]
    public void Invoke_Should_InvokeMember_When_OneOverloadMemberExists()
    {
        List<IObjectMember> objectMembers = new()
        {
            MockMember(MemberSignature.NoMatchScore, 0),
            MockMember(1, 1),
            MockMember(2, 2),
        };

        ObjectInvoker objectInvoker = new(objectMembers, _dummyInstance);

        object? invokeResult = objectInvoker.Invoke(DummyMemberName);

        Assert.That(invokeResult, Is.EqualTo(1));
    }

    [Test]
    public void Invoke_Should_ThrowMemberNotFoundException_When_NoOverloadMembersExist()
    {
        List<IObjectMember> objectMembers = new()
        {
            MockMember(MemberSignature.NoMatchScore)
        };

        ObjectInvoker objectInvoker = new(objectMembers, _dummyInstance);
        
        MemberNotFoundException exception = Assert.Throws<MemberNotFoundException>(() => objectInvoker.Invoke(DummyMemberName))!;
        Assert.That(exception.Message, Is.EqualTo($"Object member '{DummyMemberName}' for the provided arguments was not found."));
    }

    [Test]
    public void Invoke_Should_ThrowAmbiguousMemberInvokeException_When_MoreThanTwoOverloadMembersExist()
    {
        List<IObjectMember> objectMembers = new()
        {
            MockMember(MemberSignature.NoMatchScore),
            MockMember(1),
            MockMember(1),
            MockMember(1),
            MockMember(2),
        };

        ObjectInvoker objectInvoker = new(objectMembers, _dummyInstance);

        AmbiguousMemberInvokeException exception = Assert.Throws<AmbiguousMemberInvokeException>(() => objectInvoker.Invoke(DummyMemberName))!;
        Assert.That(exception.AmbiguousMembers, Has.Exactly(3).Items);
    }
}