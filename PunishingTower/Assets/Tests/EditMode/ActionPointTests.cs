using NUnit.Framework;
using PunishingTower.Combat;

namespace PunishingTower.Tests
{
    public class ActionPointTests
    {
        [Test]
        public void ActionPoints_StartAtZero()
        {
            var ap = new ActionPointSystem();

            Assert.AreEqual(0, ap.ActionPoints);
        }

        [Test]
        public void ActionPoints_BeginTurn_ResetsToThree()
        {
            var ap = new ActionPointSystem();
            ap.BeginTurn();

            Assert.AreEqual(3, ap.ActionPoints);
        }

        [Test]
        public void ActionPoints_Spend_ReducesPoints()
        {
            var ap = new ActionPointSystem();
            ap.BeginTurn();

            bool ok = ap.TrySpend(1);

            Assert.IsTrue(ok);
            Assert.AreEqual(2, ap.ActionPoints);
        }

        [Test]
        public void ActionPoints_Spend_MoreThanAvailable_Fails()
        {
            var ap = new ActionPointSystem();
            ap.BeginTurn();

            bool ok = ap.TrySpend(5);

            Assert.IsFalse(ok);
            Assert.AreEqual(3, ap.ActionPoints);
        }

        [Test]
        public void ActionPoints_DoNotCarryOver()
        {
            var ap = new ActionPointSystem();
            ap.BeginTurn();
            ap.TrySpend(1);

            ap.EndTurn();
            Assert.AreEqual(0, ap.ActionPoints);

            ap.BeginTurn();
            Assert.AreEqual(3, ap.ActionPoints);
        }

        [Test]
        public void ActionPoints_CanSpend_ReportsAffordability()
        {
            var ap = new ActionPointSystem();
            ap.BeginTurn();

            Assert.IsTrue(ap.CanSpend(3));
            Assert.IsFalse(ap.CanSpend(4));
        }
    }
}
