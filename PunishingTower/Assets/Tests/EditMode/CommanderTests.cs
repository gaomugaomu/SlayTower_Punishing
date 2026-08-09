using NUnit.Framework;
using PunishingTower.Combat;

namespace PunishingTower.Tests
{
    public class CommanderTests
    {
        [Test]
        public void Commander_InitialState_Defaults()
        {
            var commander = new CommanderState();

            Assert.AreEqual(100, commander.MaxHp);
            Assert.AreEqual(100, commander.Hp);
            Assert.AreEqual(0, commander.Infection);
            Assert.AreEqual(3, commander.SerumCount);
            Assert.IsFalse(commander.IsDefeated);
        }

        [Test]
        public void Commander_TakesDamage_ReducesHp()
        {
            var commander = new CommanderState();

            int dealt = commander.TakeDamage(30);

            Assert.AreEqual(30, dealt);
            Assert.AreEqual(70, commander.Hp);
        }

        [Test]
        public void Commander_Damage_IncreasesInfection()
        {
            var commander = new CommanderState();

            commander.TakeDamage(20);

            Assert.AreEqual(10, commander.Infection);
        }

        [Test]
        public void Commander_Shield_AbsorbsDamage()
        {
            var commander = new CommanderState();
            commander.AddShield(15);

            int dealt = commander.TakeDamage(10);

            Assert.AreEqual(0, dealt);
            Assert.AreEqual(100, commander.Hp);
            Assert.AreEqual(5, commander.Shield);
            Assert.AreEqual(0, commander.Infection);
        }

        [Test]
        public void Commander_Shield_BreaksAndRemainingHitsHp()
        {
            var commander = new CommanderState();
            commander.AddShield(10);

            int dealt = commander.TakeDamage(25);

            Assert.AreEqual(15, dealt);
            Assert.AreEqual(85, commander.Hp);
            Assert.AreEqual(0, commander.Shield);
            Assert.AreEqual(7, commander.Infection);
        }

        [Test]
        public void Commander_Heal_CapsAtMaxHp()
        {
            var commander = new CommanderState();
            commander.TakeDamage(40);
            commander.TakeDamage(40);
            commander.TakeDamage(40);

            commander.Heal(500);

            Assert.AreEqual(100, commander.Hp);
        }

        [Test]
        public void Commander_ZeroHp_IsDefeated()
        {
            var commander = new CommanderState();

            commander.TakeDamage(100);

            Assert.AreEqual(0, commander.Hp);
            Assert.IsTrue(commander.IsDefeated);
        }

        [Test]
        public void Commander_CriticalInfection_IsDefeated()
        {
            var commander = new CommanderState();

            commander.AddInfection(100);

            Assert.IsTrue(commander.IsDefeated);
        }

        [Test]
        public void Commander_UseSerum_ReducesInfectionAndConsumes()
        {
            var commander = new CommanderState();
            commander.AddInfection(60);

            bool success = commander.UseSerum();

            Assert.IsTrue(success);
            Assert.AreEqual(35, commander.Infection);
            Assert.AreEqual(2, commander.SerumCount);
        }

        [Test]
        public void Commander_UseSerum_UnavailableWhenEmpty()
        {
            var commander = new CommanderState(serumCount: 0);
            commander.AddInfection(20);

            bool success = commander.UseSerum();

            Assert.IsFalse(success);
            Assert.AreEqual(20, commander.Infection);
        }

        [Test]
        public void Commander_LowInfection_NoTurnStartPenalty()
        {
            var commander = new CommanderState();
            commander.AddInfection(10);

            int penalty = commander.ApplyTurnStartPenalty();

            Assert.AreEqual(0, penalty);
            Assert.AreEqual(100, commander.Hp);
        }

        [Test]
        public void Commander_MediumInfection_TurnStartPenalty()
        {
            var commander = new CommanderState();
            commander.AddInfection(40);

            int penalty = commander.ApplyTurnStartPenalty();

            Assert.AreEqual(1, penalty);
            Assert.AreEqual(99, commander.Hp);
        }

        [Test]
        public void Commander_HighInfection_StrongerPenalty()
        {
            var commander = new CommanderState();
            commander.AddInfection(70);

            int penalty = commander.ApplyTurnStartPenalty();

            Assert.AreEqual(3, penalty);
            Assert.AreEqual(97, commander.Hp);
        }

        [Test]
        public void Commander_InfectionLevel_Thresholds()
        {
            var low = new CommanderState();
            low.AddInfection(33);
            Assert.AreEqual(InfectionLevel.Low, low.GetInfectionLevel());

            var medium = new CommanderState();
            medium.AddInfection(34);
            Assert.AreEqual(InfectionLevel.Medium, medium.GetInfectionLevel());

            var high = new CommanderState();
            high.AddInfection(67);
            Assert.AreEqual(InfectionLevel.High, high.GetInfectionLevel());

            var critical = new CommanderState();
            critical.AddInfection(100);
            Assert.AreEqual(InfectionLevel.Critical, critical.GetInfectionLevel());
        }
    }
}
