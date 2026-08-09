using NUnit.Framework;
using PunishingTower.Core.Events;

namespace PunishingTower.Tests
{
    public class EventBusTests
    {
        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
        }

        [Test]
        public void Publish_DeliversToTypedSubscriber()
        {
            int received = 0;
            EventBus.Subscribe<TurnStartEvent>(e => received = e.Round);

            EventBus.Publish(new TurnStartEvent(3));

            Assert.AreEqual(3, received);
        }

        [Test]
        public void Publish_DoesNotDeliverToOtherTypes()
        {
            int received = 0;
            EventBus.Subscribe<TurnEndEvent>(e => received++);

            EventBus.Publish(new TurnStartEvent(1));

            Assert.AreEqual(0, received);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            int received = 0;
            System.Action<TurnStartEvent> handler = e => received++;
            EventBus.Subscribe(handler);

            EventBus.Unsubscribe(handler);
            EventBus.Publish(new TurnStartEvent(1));

            Assert.AreEqual(0, received);
        }

        [Test]
        public void SubscribeAll_ReceivesEveryEvent()
        {
            int received = 0;
            EventBus.SubscribeAll(e => received++);

            EventBus.Publish(new TurnStartEvent(1));
            EventBus.Publish(new BattleStartEvent(1));
            EventBus.Publish(new OrbPlayedEvent("o1", 0));

            Assert.AreEqual(3, received);
        }

        [Test]
        public void MultipleSubscribers_AllReceiveEvent()
        {
            int first = 0;
            int second = 0;
            EventBus.Subscribe<TurnStartEvent>(e => first = e.Round);
            EventBus.Subscribe<TurnStartEvent>(e => second = e.Round);

            EventBus.Publish(new TurnStartEvent(5));

            Assert.AreEqual(5, first);
            Assert.AreEqual(5, second);
        }

        [Test]
        public void Clear_RemovesAllHandlers()
        {
            int received = 0;
            EventBus.Subscribe<TurnStartEvent>(e => received++);

            EventBus.Clear();
            EventBus.Publish(new TurnStartEvent(1));

            Assert.AreEqual(0, received);
        }

        [Test]
        public void Event_PreservesAllPayload()
        {
            var evt = new DamageTakenEvent("enemy_1", 12);

            Assert.AreEqual("enemy_1", evt.TargetId);
            Assert.AreEqual(12, evt.Amount);
        }
    }
}
