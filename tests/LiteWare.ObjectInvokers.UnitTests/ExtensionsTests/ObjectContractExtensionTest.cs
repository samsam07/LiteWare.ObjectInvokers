using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteWare.ObjectInvokers.Attributes;
using LiteWare.ObjectInvokers.Extensions;
using Moq;
using NUnit.Framework;

#pragma warning disable CS0067
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
        public event EventHandler<int>? IgnoredBaseEvent;

        [Listenable(CustomObjectMemberName)]
        public event Action? BaseEvent;

        public bool UnInvokableField;

        [Invokable]
        public static int BaseProperty { get; set; }
    }

    private class StubClass : StubClassBase
    {
        public const string PrivateFieldName = nameof(_privateField);

        [Listenable]
        private event Action? PrivateEvent;

        [Invokable]
        private bool _privateField;

        [Invokable(CustomObjectMemberName)]
        public static int Property { get; set; }

        [Invokable]
        public string Method() => nameof(Method);

        public bool UnInvokableMethod() => false;
    }

    #endregion

#pragma warning disable CS8618
    private Mock<IEventNotifier> _eventNotifierMock;
#pragma warning restore CS8618

    [SetUp]
    public void Setup()
    {
        _eventNotifierMock = new Mock<IEventNotifier>();
    }

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
    public void ExtractInvokableMembers_Should_ReturnMembersAsDescribedByPredicateAndSelector()
    {
        const string testName = "TestName";
        IEnumerable<IObjectMember> objectMembers = typeof(StubClass)
            .ExtractInvokableMembers
            (
                m => m.Name == nameof(StubClass.UnInvokableMethod),
                m => testName
            )
            .ToList();

        Assert.That(objectMembers, Has.One.Items, "Object member count");
        Assert.That(objectMembers.First().PreferredName, Is.EqualTo(testName), "Object member preferred name");
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

    [Test]
    public void ExtractEventSubscriberDelegates_Should_ReturnEventsAsDescribedByPredicate()
    {
        var objectEvents = typeof(StubClass)
            .ExtractEventSubscriberDelegates
            (
                _eventNotifierMock.Object,
                m => m.Name == "IgnoredBaseEvent"
            )
            .Select(kp => kp.Key)
            .ToList();

        Assert.That(objectEvents, Has.One.Items, "Object event count");
        Assert.That(objectEvents.First().Name, Is.EqualTo("IgnoredBaseEvent"), "Object event name");
    }

    [Test]
    public void FindEventSubscriberDelegates_Should_ReturnAllEventsQualifiedByListenableAttribute()
    {
        var objectEvents = typeof(StubClass)
            .FindEventSubscriberDelegates(_eventNotifierMock.Object)
            .Select(kp => kp.Key)
            .ToList();

        Assert.That(objectEvents, Has.Exactly(2).Items, "Object member count");
        Assert.That(objectEvents, Has.One.With.Property(nameof(EventInfo.Name)).EqualTo("PrivateEvent"), "Found private event in class");
        Assert.That(objectEvents, Has.One.With.Property(nameof(EventInfo.Name)).EqualTo("BaseEvent"), "Found event in base class");
    }

    [Test]
    public void CreateSubscriberDelegate_Should_CreateValidDelegate()
    {
        EventInfo eventInfo = typeof(StubClassBase).GetEvent("BaseEvent")!;

        Delegate @delegate = eventInfo.CreateSubscriberDelegate(_eventNotifierMock.Object);
        @delegate.DynamicInvoke();

        _eventNotifierMock.Verify(e => e.NotifyEvent("BaseEvent", new object[] { }));
    }

    [Test]
    public void CreateSubscriberDelegate_Should_CreateValidDelegateWithCustomEventName_When_PreferredNameIsProvided()
    {
        EventInfo eventInfo = typeof(StubClassBase).GetEvent("IgnoredBaseEvent")!;

        Delegate @delegate = eventInfo.CreateSubscriberDelegate(_eventNotifierMock.Object, "TestEvent");
        @delegate.DynamicInvoke(1, 2);

        _eventNotifierMock.Verify(e => e.NotifyEvent("TestEvent", new object[] { 1, 2 }));
    }
}