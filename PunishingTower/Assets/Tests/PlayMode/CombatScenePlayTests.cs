using System.Collections;
using NUnit.Framework;
using PunishingTower.Combat;
using PunishingTower.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PunishingTower.Tests
{
    /// <summary>
    /// PlayMode tests: load the integrated CombatTest scene and drive a full battle
    /// through the public driver API (selection, attack, orbs, ultimate, end turn, victory).
    /// </summary>
    public class CombatScenePlayTests
    {
        private const string SceneName = "CombatTest";

        private static IEnumerator LoadScene()
        {
            if (!Application.isPlaying)
            {
                Assert.Ignore("PlayMode-only test - skipped when collected by the EditMode platform");
                yield break;
            }

            SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
            yield return null;
            yield return null;
        }

        private static CombatTestDriver FindDriver()
        {
            var driver = Object.FindObjectOfType<CombatTestDriver>();
            Assert.IsNotNull(driver, "CombatTestDriver not found in scene");
            return driver;
        }

        [UnityTest]
        public IEnumerator Battle_Starts_WithFullState()
        {
            yield return LoadScene();
            CombatTestDriver driver = FindDriver();

            Assert.AreEqual(3, driver.SquadCount);
            Assert.AreEqual(2, driver.EnemyCount);
            Assert.AreEqual(3, driver.MaxActionPoints);
            Assert.AreEqual(3, driver.CurrentActionPoints);
            Assert.AreEqual(100, driver.CommanderHp);
            Assert.AreEqual(1, driver.Round);
            Assert.IsFalse(driver.IsBattleOver);
        }

        [UnityTest]
        public IEnumerator BasicAttack_SelectedConstruct_DamagesSelectedEnemy()
        {
            yield return LoadScene();
            CombatTestDriver driver = FindDriver();

            string firstEnemy = driver.SelectedEnemyName;
            int hpBefore = driver.GetEnemy(0).Hp;

            driver.ActionBasicAttack();

            Assert.AreEqual(2, driver.CurrentActionPoints, "AP should drop by 1");
            Assert.Less(driver.GetEnemy(0).Hp, hpBefore, "enemy should lose HP");
        }

        [UnityTest]
        public IEnumerator SwitchConstruct_ChangesSelected()
        {
            yield return LoadScene();
            CombatTestDriver driver = FindDriver();

            string first = driver.SelectedConstructName;

            driver.ActionSelectNextConstruct();
            string second = driver.SelectedConstructName;

            Assert.AreNotEqual(first, second, "selection should change");

            driver.ActionSelectNextConstruct();
            driver.ActionSelectNextConstruct();
            Assert.AreEqual(first, driver.SelectedConstructName, "selection should wrap around");
        }

        [UnityTest]
        public IEnumerator SwitchEnemy_ChangesTarget()
        {
            yield return LoadScene();
            CombatTestDriver driver = FindDriver();

            string first = driver.SelectedEnemyName;

            driver.ActionSelectNextEnemy();
            string second = driver.SelectedEnemyName;

            Assert.AreNotEqual(first, second, "target should change");
        }

        [UnityTest]
        public IEnumerator PlayOrb_ExecutesSkill_AndDiscards()
        {
            yield return LoadScene();
            CombatTestDriver driver = FindDriver();

            int handBefore = driver.HandCount;
            int discardBefore = driver.DiscardCount;

            // Click the rightmost visible orb (slot 0).
            driver.ActionPlayOrb(0);

            Assert.IsTrue(driver.HandCount < handBefore || driver.DiscardCount > discardBefore,
                "playing an orb should remove it from hand and move it to discard");
            Assert.AreEqual(2, driver.CurrentActionPoints, "orb play should cost 1 AP");
        }

        [UnityTest]
        public IEnumerator Ultimate_WhenEnergyFull_DealsDamage()
        {
            yield return LoadScene();
            CombatTestDriver driver = FindDriver();

            // Constructs start with full energy (100).
            int enemy1Hp = driver.GetEnemy(0).Hp;

            driver.ActionUltimate();

            Assert.Less(driver.GetEnemy(0).Hp, enemy1Hp, "ultimate should damage all enemies");
            Assert.AreEqual(2, driver.CurrentActionPoints, "ultimate should cost 1 AP");
        }

        [UnityTest]
        public IEnumerator EndTurn_AdvancesRound_AndEnemyActs()
        {
            yield return LoadScene();
            CombatTestDriver driver = FindDriver();

            int roundBefore = driver.Round;
            int commanderHpBefore = driver.CommanderHp;

            driver.ActionEndTurn();

            Assert.AreEqual(roundBefore + 1, driver.Round, "round should advance");
            Assert.LessOrEqual(driver.CommanderHp, commanderHpBefore, "enemy should act (attack or the commander takes no damage if shield)");
        }

        [UnityTest]
        public IEnumerator KillAllEnemies_Victory()
        {
            yield return LoadScene();
            CombatTestDriver driver = FindDriver();

            // Basic attacks until every enemy dies, ending turns to restore AP.
            int guard = 0;
            while (!driver.IsBattleOver && guard < 60)
            {
                if (driver.GetEnemy(0).Hp > 0 || driver.GetEnemy(1).Hp > 0)
                {
                    if (driver.CurrentActionPoints < 1)
                    {
                        driver.ActionEndTurn();
                        yield return null;
                    }
                    else
                    {
                        driver.ActionBasicAttack();
                    }
                }
                guard++;
                yield return null;
            }

            Assert.IsTrue(driver.IsBattleOver, "battle should be over after all enemies die");
        }

        [UnityTest]
        public IEnumerator FullBattle_FirstPlayableScenario()
        {
            yield return LoadScene();
            CombatTestDriver driver = FindDriver();

            // 1. Battle starts: orbs drawn, AP ready.
            Assert.AreEqual(8, driver.HandCount, "orbs should be drawn at battle start");
            Assert.AreEqual(3, driver.CurrentActionPoints);

            // 2. Play an orb: AP decreases, hand shrinks.
            int handBefore = driver.HandCount;
            driver.ActionPlayOrb(0);
            Assert.AreEqual(2, driver.CurrentActionPoints, "AP decreases after orb play");
            Assert.Less(driver.HandCount, handBefore, "orb removed from hand");

            // 3. Enemy attacks after ending turn: commander HP may drop, infection may rise.
            int hpBefore = driver.CommanderHp;
            int infectionBefore = driver.CommanderInfection;
            driver.ActionEndTurn();
            Assert.LessOrEqual(driver.CommanderHp, hpBefore, "enemy attacks commander");
            Assert.GreaterOrEqual(driver.CommanderInfection, infectionBefore, "damage raises infection");

            // 4. Fight until victory: enemies die, rewards appear.
            int guard = 0;
            while (!driver.IsBattleOver && guard < 80)
            {
                if (driver.GetEnemy(0).Hp > 0 || driver.GetEnemy(1).Hp > 0)
                {
                    if (driver.CurrentActionPoints < 1)
                    {
                        driver.ActionEndTurn();
                    }
                    else
                    {
                        driver.ActionBasicAttack();
                    }
                }
                guard++;
                yield return null;
            }

            Assert.IsTrue(driver.IsBattleOver, "battle ends in victory");
        }

        [UnityTest]
        public IEnumerator Victory_GrantsRewards()
        {
            yield return LoadScene();
            CombatTestDriver driver = FindDriver();

            int guard = 0;
            while (!driver.IsBattleOver && guard < 80)
            {
                if (driver.GetEnemy(0).Hp > 0 || driver.GetEnemy(1).Hp > 0)
                {
                    if (driver.CurrentActionPoints < 1)
                    {
                        driver.ActionEndTurn();
                    }
                    else
                    {
                        driver.ActionBasicAttack();
                    }
                }
                guard++;
                yield return null;
            }

            Assert.IsTrue(driver.IsBattleOver, "battle should end");
            Assert.IsTrue(driver.LastMessage.Contains("奖励"), "victory message should include rewards");
        }
        [UnityTest]
        public IEnumerator BattleUiScene_BuildsHud()
        {
            SceneManager.LoadScene("CombatUiTest", LoadSceneMode.Single);
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            var hud = Object.FindObjectOfType<BattleHud>();
            Assert.IsNotNull(hud, "BattleHud should exist in CombatUiTest scene");

            var driver = Object.FindObjectOfType<CombatTestDriver>();
            Assert.IsNotNull(driver, "CombatTestDriver should exist");

            // Give the HUD a couple more frames to initialize.
            yield return null;
            yield return null;

            // Canvas with UI text must exist.
            var canvas = Object.FindObjectOfType<Canvas>();
            Assert.IsNotNull(canvas, "Canvas should be created by BattleHud");

            // UI texts (Chinese labels) must exist and be non-empty.
            var texts = canvas.GetComponentsInChildren<TMP_Text>(true);
            Assert.Greater(texts.Length, 0, "HUD should contain TMP texts");

            // Art sprites must load from Resources (null sprites render as white boxes).
            Assert.IsNotNull(Resources.Load<Sprite>("Art/UI_Panel_Dark"), "panel sprite must load");
            Assert.IsNotNull(Resources.Load<Sprite>("Art/UI_Card_Orb_Red"), "orb card sprite must load");
            Assert.IsNotNull(Resources.Load<Sprite>("Art/UI_Icon_Orb_Red"), "orb icon sprite must load");
            Assert.IsNotNull(Resources.Load<Sprite>("Art/UI_Icon_Intent_Attack"), "intent sprite must load");

            // EventSystem is required for all UGUI clicks (buttons / orb cards).
            Assert.IsNotNull(Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>(),
                "EventSystem must exist for pointer input");
            Assert.IsNotNull(Object.FindObjectOfType<UnityEngine.UI.GraphicRaycaster>(),
                "GraphicRaycaster must exist on the Canvas for pointer input");
        }
    }
}
