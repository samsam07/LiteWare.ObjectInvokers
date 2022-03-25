using System;
using LiteWare.ObjectInvokers.Attributes;
using Moq;
using NUnit.Framework;

namespace LiteWare.ObjectInvokers.UnitTests;

[TestFixture]
public class EventListenerTest
{
    #region Stubs

    private class StubEventArgs : EventArgs
    {
        public int Id { get; set; }

        public StubEventArgs(int id)
        {
            Id = id;
        }
    }

    private class StubClass
    {
        [Listenable]
        public event EventHandler<StubEventArgs>? Event1;

        public void RaiseEvent1(StubEventArgs e)
        {
            Event1?.Invoke(this, e);
        }
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

    [Test]
    public void Bind_Should_ConstructEventListenerWithMarkedMembers()
    {
        StubEventArgs e1 = new(1);

        StubClass stubClass = new();
        using EventListener eventListener = EventListener.Bind(stubClass, _eventNotifierMock.Object);
        eventListener.StartListening();

        stubClass.RaiseEvent1(e1);
        _eventNotifierMock.Verify(e => e.NotifyEvent("Event1", new object[] { stubClass, e1 }));
    }

    [Test]
    public void StopListening_Should_NotBroadcastEventNotifications()
    {
        StubEventArgs e1 = new(1);

        StubClass stubClass = new();
        using EventListener eventListener = EventListener.Bind(stubClass, _eventNotifierMock.Object);
        eventListener.StartListening();
        eventListener.StopListening();

        stubClass.RaiseEvent1(e1);
        _eventNotifierMock.Verify(e => e.NotifyEvent(It.IsAny<string>(), It.IsAny<object?[]>()), Times.Never);
    }
}