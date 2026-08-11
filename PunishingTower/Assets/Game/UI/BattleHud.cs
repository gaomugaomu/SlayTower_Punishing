using System.Collections.Generic;
using PunishingTower.Combat;
using PunishingTower.Construct;
using PunishingTower.Data;
using PunishingTower.SignalOrb;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PunishingTower.UI
{
    /// <summary>
    /// UGUI combat HUD (Slay the Spire layout). Builds its entire UI hierarchy in code
    /// and drives it from a CombatTestDriver. UI only reads state and forwards actions -
    /// it never modifies combat logic directly (doc 166).
    /// </summary>
    public class BattleHud : MonoBehaviour
    {
        [SerializeField] private CombatTestDriver driver;

        private bool uiBuilt;

        private readonly List<GameObject> enemyCards = new List<GameObject>();
        private readonly List<GameObject> constructCards = new List<GameObject>();
        private readonly List<OrbCardView> orbCards = new List<OrbCardView>();

        private TMP_FontAsset font;
        private Sprite cardRed, cardYellow, cardBlue, cardWhite, cardCorrupted;
        private Sprite iconRed, iconYellow, iconBlue, iconWhite, iconCorrupted;
        private Sprite intentAttack, intentDefense, intentBuff, intentDebuff, intentSpecial;
        private Sprite panelDark;

        private Image commanderHpFill, commanderInfectionFill;
        private TMP_Text commanderLabel, apLabel, roundLabel, serumLabel, lastLabel, deckLabel, handLabel;

        private void Start()
        {
            StartCoroutine(InitializeWhenReady());
        }

        /// <summary>
        /// Waits until the driver finished its own Start (battle/squad/orbPool exist)
        /// before building the UI, because Start order between components is undefined.
        /// </summary>
        private System.Collections.IEnumerator InitializeWhenReady()
        {
            if (driver == null)
            {
                driver = GetComponent<CombatTestDriver>();
            }
            if (driver == null)
            {
                driver = FindObjectOfType<CombatTestDriver>();
            }
            if (driver == null)
            {
                Debug.LogError("BattleHud: no CombatTestDriver found");
                yield break;
            }

            int guard = 0;
            while (!driver.IsReady && guard < 120)
            {
                guard++;
                yield return null;
            }

            if (!driver.IsReady)
            {
                Debug.LogError("BattleHud: driver did not become ready");
                yield break;
            }

            LoadAssets();
            BuildUi();
            uiBuilt = true;
        }

        private void Update()
        {
            if (driver == null || !uiBuilt)
            {
                return;
            }
            RefreshAll();
        }

        // ---------------- Asset loading ----------------

        private void LoadAssets()
        {
            // All art lives under Resources/Art so Resources.Load works at runtime reliably.
            font = Resources.Load<TMP_FontAsset>("Fonts/SimHei SDF") ?? LoadFontAtPath();
            cardRed = Resources.Load<Sprite>("Art/UI_Card_Orb_Red");
            cardYellow = Resources.Load<Sprite>("Art/UI_Card_Orb_Yellow");
            cardBlue = Resources.Load<Sprite>("Art/UI_Card_Orb_Blue");
            cardWhite = Resources.Load<Sprite>("Art/UI_Card_Orb_White");
            cardCorrupted = Resources.Load<Sprite>("Art/UI_Card_Orb_Corrupted");
            iconRed = Resources.Load<Sprite>("Art/UI_Icon_Orb_Red");
            iconYellow = Resources.Load<Sprite>("Art/UI_Icon_Orb_Yellow");
            iconBlue = Resources.Load<Sprite>("Art/UI_Icon_Orb_Blue");
            iconWhite = Resources.Load<Sprite>("Art/UI_Icon_Orb_White");
            iconCorrupted = Resources.Load<Sprite>("Art/UI_Icon_Orb_Corrupted");
            intentAttack = Resources.Load<Sprite>("Art/UI_Icon_Intent_Attack");
            intentDefense = Resources.Load<Sprite>("Art/UI_Icon_Intent_Defense");
            intentBuff = Resources.Load<Sprite>("Art/UI_Icon_Intent_Buff");
            intentDebuff = Resources.Load<Sprite>("Art/UI_Icon_Intent_Debuff");
            intentSpecial = Resources.Load<Sprite>("Art/UI_Icon_Intent_Special");
            panelDark = Resources.Load<Sprite>("Art/UI_Panel_Dark");

            if (font == null)
            {
                Debug.LogError("BattleHud: SimHei SDF font not found (中文将显示为方块)");
            }
            if (panelDark == null)
            {
                Debug.LogError("BattleHud: UI_Panel_Dark not found in Resources/Art - panels will render white");
            }
        }

        private static TMP_FontAsset LoadFontAtPath()
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/UI/Resources/Fonts/SimHei SDF.asset");
        }

        // ---------------- UI construction ----------------

        private void BuildUi()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // --- Top: enemies ---
            BuildEnemyArea(canvasGo.transform);

            // --- Left: commander panel ---
            BuildCommanderPanel(canvasGo.transform);

            // --- Right: action area ---
            BuildActionArea(canvasGo.transform);

            // --- Construct row (above orb row) ---
            BuildConstructRow(canvasGo.transform);

            // --- Bottom: orb cards ---
            BuildOrbRow(canvasGo.transform);

            // --- Log line ---
            var logRect = CreateRect(canvasGo.transform, "Log", new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f),
                new Vector2(0, 0), new Vector2(1400, 40));
            lastLabel = CreateText(logRect, "Last", string.Empty, 22, TextAlignmentOptions.Center);
        }

        private void BuildEnemyArea(Transform canvas)
        {
            var area = CreateRect(canvas, "Enemies", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -20), new Vector2(1200, 180));

            int count = driver.EnemyCount;
            for (int i = 0; i < count; i++)
            {
                var card = CreateRect(area, "Enemy_" + i, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2((i - (count - 1) / 2f) * 260, 0), new Vector2(230, 170));

                var bg = card.gameObject.AddComponent<Image>();
                bg.sprite = panelDark;
                bg.type = Image.Type.Simple;

                var hpFill = CreateRect(card.transform, "HpFill", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0, 14), new Vector2(200, 16));
                var hpImage = hpFill.gameObject.AddComponent<Image>();
                hpImage.color = new Color(0.75f, 0.2f, 0.2f);
                hpImage.type = Image.Type.Filled;
                hpImage.fillMethod = Image.FillMethod.Horizontal;
                hpImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                hpFill.name = "HpFill";
                hpFill.gameObject.AddComponent<HudFillRef>().fill = hpImage;

                var hpText = CreateText(card.transform, "HpText", "50/50", 20, TextAlignmentOptions.Center);
                var hpTextRect = hpText.GetComponent<RectTransform>();
                hpTextRect.anchorMin = new Vector2(0.5f, 0f);
                hpTextRect.anchorMax = new Vector2(0.5f, 0f);
                hpTextRect.pivot = new Vector2(0.5f, 0.5f);
                hpTextRect.anchoredPosition = new Vector2(0, 14);
                hpTextRect.sizeDelta = new Vector2(200, 16);

                var intent = CreateRect(card.transform, "Intent", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0, -10), new Vector2(56, 56));
                var intentImage = intent.gameObject.AddComponent<Image>();
                intentImage.sprite = intentAttack;

                var nameText = CreateText(card.transform, "Name", "Enemy", 22, TextAlignmentOptions.Center);
                var nameRect = nameText.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0.5f, 0.8f);
                nameRect.anchorMax = new Vector2(0.5f, 0.8f);
                nameRect.pivot = new Vector2(0.5f, 0.5f);
                nameRect.anchoredPosition = Vector2.zero;
                nameRect.sizeDelta = new Vector2(210, 30);

                var shieldText = CreateText(card.transform, "Shield", "", 18, TextAlignmentOptions.Center);
                var shieldRect = shieldText.GetComponent<RectTransform>();
                shieldRect.anchorMin = new Vector2(0.5f, 0.35f);
                shieldRect.anchorMax = new Vector2(0.5f, 0.35f);
                shieldRect.pivot = new Vector2(0.5f, 0.5f);
                shieldRect.anchoredPosition = Vector2.zero;
                shieldRect.sizeDelta = new Vector2(200, 20);

                enemyCards.Add(card.gameObject);
            }
        }

        private void BuildCommanderPanel(Transform canvas)
        {
            var panel = CreateRect(canvas, "Commander", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(20, 0), new Vector2(320, 300));
            var bg = panel.gameObject.AddComponent<Image>();
            bg.sprite = panelDark;
            bg.type = Image.Type.Simple;

            var title = CreateText(panel.transform, "Title", "指挥官", 24, TextAlignmentOptions.Center);
            PlaceCentered(title, new Vector2(0.5f, 0.92f), new Vector2(280, 30));

            commanderLabel = CreateText(panel.transform, "Hp", "", 22, TextAlignmentOptions.Left);
            PlaceCentered(commanderLabel, new Vector2(0.5f, 0.78f), new Vector2(280, 24));

            commanderHpFill = CreateBar(panel.transform, "HpBar", new Color(0.75f, 0.2f, 0.2f), new Vector2(0.5f, 0.68f));
            commanderInfectionFill = CreateBar(panel.transform, "InfectionBar", new Color(0.6f, 0.3f, 0.8f), new Vector2(0.5f, 0.58f));

            serumLabel = CreateText(panel.transform, "Serum", "", 20, TextAlignmentOptions.Left);
            PlaceCentered(serumLabel, new Vector2(0.5f, 0.47f), new Vector2(280, 22));

            deckLabel = CreateText(panel.transform, "Deck", "", 18, TextAlignmentOptions.Left);
            PlaceCentered(deckLabel, new Vector2(0.5f, 0.36f), new Vector2(280, 44));
        }

        private void BuildActionArea(Transform canvas)
        {
            var panel = CreateRect(canvas, "Actions", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-20, 0), new Vector2(340, 360));
            var bg = panel.gameObject.AddComponent<Image>();
            bg.sprite = panelDark;
            bg.type = Image.Type.Simple;

            roundLabel = CreateText(panel.transform, "Round", "", 24, TextAlignmentOptions.Center);
            PlaceCentered(roundLabel, new Vector2(0.5f, 0.92f), new Vector2(300, 30));

            apLabel = CreateText(panel.transform, "AP", "", 22, TextAlignmentOptions.Center);
            PlaceCentered(apLabel, new Vector2(0.5f, 0.84f), new Vector2(300, 26));

            float y = 0.72f;
            CreateButton(panel.transform, "普攻 (1AP)", new Vector2(0.5f, y), () => driver.ActionBasicAttack());
            y -= 0.09f;
            CreateButton(panel.transform, "大招 R (1AP)", new Vector2(0.5f, y), () => driver.ActionUltimate());
            y -= 0.09f;
            CreateButton(panel.transform, "使用血清", new Vector2(0.5f, y), () => driver.ActionUseSerum());
            y -= 0.09f;
            CreateButton(panel.transform, "结束回合", new Vector2(0.5f, y), () => driver.ActionEndTurn());
            y -= 0.09f;
            CreateButton(panel.transform, "Boss 模式切换", new Vector2(0.5f, y), () => driver.ToggleBossMode());
            y -= 0.09f;
            CreateButton(panel.transform, "重新开始", new Vector2(0.5f, y), () => driver.ActionRestart());
        }

        private void BuildConstructRow(Transform canvas)
        {
            var row = CreateRect(canvas, "Constructs", new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f),
                new Vector2(0, 0), new Vector2(1000, 150));

            int count = driver.SquadCount;
            for (int i = 0; i < count; i++)
            {
                var card = CreateRect(row, "Construct_" + i, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2((i - (count - 1) / 2f) * 330, 0), new Vector2(300, 140));
                var bg = card.gameObject.AddComponent<Image>();
                bg.sprite = panelDark;
                bg.type = Image.Type.Simple;

                var name = CreateText(card.transform, "Name", "", 22, TextAlignmentOptions.Center);
                PlaceCentered(name, new Vector2(0.5f, 0.88f), new Vector2(280, 26));

                var energy = CreateText(card.transform, "Energy", "", 18, TextAlignmentOptions.Left);
                PlaceCentered(energy, new Vector2(0.5f, 0.62f), new Vector2(280, 22));

                CreateBar(card.transform, "EnergyBar", new Color(0.25f, 0.55f, 0.9f), new Vector2(0.5f, 0.48f));

                var relics = CreateText(card.transform, "Relics", "", 16, TextAlignmentOptions.Left);
                PlaceCentered(relics, new Vector2(0.5f, 0.3f), new Vector2(280, 30));

                constructCards.Add(card.gameObject);
            }
        }

        private void BuildOrbRow(Transform canvas)
        {
            var row = CreateRect(canvas, "OrbRow", new Vector2(0.5f, 0.06f), new Vector2(0.5f, 0.06f),
                new Vector2(0, 0), new Vector2(900, 180));

            // Hand counter (0/16) above the orb cards.
            handLabel = CreateText(row, "HandCount", "手牌 0/16", 18, TextAlignmentOptions.Center);
            var handRt = handLabel.GetComponent<RectTransform>();
            handRt.anchorMin = new Vector2(0.5f, 1f);
            handRt.anchorMax = new Vector2(0.5f, 1f);
            handRt.pivot = new Vector2(0.5f, 1f);
            handRt.anchoredPosition = Vector2.zero;
            handRt.sizeDelta = new Vector2(300, 24);

            // Load the OrbCard prefab and instantiate 8 cards from right to left.
            var prefab = Resources.Load<GameObject>("UI/OrbCard") ?? LoadPrefabAtPath();
            for (int i = 0; i < 8; i++)
            {
                GameObject cardGo;
                if (prefab != null)
                {
                    cardGo = Instantiate(prefab, row.transform);
                    cardGo.name = "Orb_" + i;
                }
                else
                {
                    cardGo = CreateRect(row, "Orb_" + i, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        Vector2.zero, new Vector2(96, 132)).gameObject;
                }

                var rt = cardGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                // Rightmost = slot 0 (oldest); place slots left-to-right with slot 0 at the right.
                rt.anchoredPosition = new Vector2((i - 3.5f) * 106, 0);

                var view = cardGo.GetComponent<OrbCardView>();
                if (view == null)
                {
                    view = cardGo.gameObject.AddComponent<OrbCardView>();
                }
                orbCards.Add(view);
            }
        }

        // ---------------- Refresh ----------------

        private void RefreshAll()
        {
            RefreshEnemies();
            RefreshCommander();
            RefreshActions();
            RefreshConstructs();
            RefreshOrbs();
        }

        private void RefreshEnemies()
        {
            for (int i = 0; i < enemyCards.Count && i < driver.EnemyCount; i++)
            {
                EnemyState enemy = driver.GetEnemy(i);
                GameObject card = enemyCards[i];

                var bg = card.GetComponent<Image>();
                bool selected = driver.SelectedEnemyName == enemy.DisplayName;
                bg.color = selected ? new Color(1f, 0.7f, 0.7f, 0.95f) : Color.white;

                var hpFill = card.transform.Find("HpFill")?.GetComponent<HudFillRef>();
                if (hpFill != null)
                {
                    hpFill.fill.fillAmount = enemy.MaxHp > 0 ? enemy.Hp / (float)enemy.MaxHp : 0f;
                }

                var hpText = card.transform.Find("HpText")?.GetComponent<TMP_Text>();
                if (hpText != null)
                {
                    hpText.text = $"{enemy.Hp}/{enemy.MaxHp}";
                }

                var shieldText = card.transform.Find("Shield")?.GetComponent<TMP_Text>();
                if (shieldText != null)
                {
                    shieldText.text = enemy.Shield > 0 ? $"盾 {enemy.Shield}" : string.Empty;
                }

                var nameText = card.transform.Find("Name")?.GetComponent<TMP_Text>();
                if (nameText != null)
                {
                    nameText.text = enemy.IsDefeated ? enemy.DisplayName + " (已败)" : enemy.DisplayName;
                }

                var intent = card.transform.Find("Intent")?.GetComponent<Image>();
                if (intent != null)
                {
                    intent.sprite = enemy.IsDefeated ? intentSpecial : PickIntentSprite(enemy.AttackDamage);
                }
            }
        }

        private Sprite PickIntentSprite(int attack)
        {
            // Simple mapping for the demo enemy AI: alternating turns are handled by the driver;
            // we display attack intent for alive enemies.
            return intentAttack;
        }

        private void RefreshCommander()
        {
            if (commanderLabel != null)
            {
                commanderLabel.text = $"HP {driver.CommanderHp}/{100}  盾 {driver.GetCommanderShield() ?? 0}";
            }
            if (commanderHpFill != null)
            {
                commanderHpFill.fillAmount = driver.CommanderHp / 100f;
            }
            if (commanderInfectionFill != null)
            {
                commanderInfectionFill.fillAmount = driver.CommanderInfection / 100f;
            }
            if (serumLabel != null)
            {
                serumLabel.text = $"血清 {driver.SerumCount} | 感染 {driver.CommanderInfection}/100";
            }
            if (deckLabel != null)
            {
                deckLabel.text = $"牌组 {driver.HandCount}手 | {driver.DiscardCount}弃 | 锁球 {driver.CountLockedOrbs()}";
            }
        }

        private void RefreshActions()
        {
            if (roundLabel != null)
            {
                roundLabel.text = $"第 {driver.Round} 回合";
            }
            if (apLabel != null)
            {
                apLabel.text = $"行动点 {driver.CurrentActionPoints}/{driver.MaxActionPoints}";
            }
            if (lastLabel != null)
            {
                lastLabel.text = driver.LastMessage;
            }
        }

        private void RefreshConstructs()
        {
            for (int i = 0; i < constructCards.Count && i < driver.SquadCount; i++)
            {
                ConstructState c = driver.GetConstruct(i);
                GameObject card = constructCards[i];
                card.GetComponent<Image>().color = i == driver.GetSelectedConstructIndex()
                    ? new Color(1f, 0.9f, 0.5f, 0.95f)
                    : Color.white;

                var name = card.transform.Find("Name")?.GetComponent<TMP_Text>();
                if (name != null)
                {
                    string state = c.IsActive ? "" : (c.Flag == ConstructStateFlag.Unavailable ? " [离队]" : " [恢复中]");
                    name.text = c.DisplayName + state;
                }

                var energy = card.transform.Find("Energy")?.GetComponent<TMP_Text>();
                if (energy != null)
                {
                    energy.text = $"能量 {c.Energy}/{c.EnergyMax}";
                }

                var bar = card.transform.Find("EnergyBar")?.GetComponent<HudFillRef>();
                if (bar != null)
                {
                    bar.fill.fillAmount = c.EnergyMax > 0 ? c.Energy / (float)c.EnergyMax : 0f;
                }

                var relics = card.transform.Find("Relics")?.GetComponent<TMP_Text>();
                if (relics != null)
                {
                    if (c.EquippedRelics.Count > 0)
                    {
                        var names = new List<string>();
                        foreach (RelicData r in c.EquippedRelics)
                        {
                            names.Add(r.DisplayName);
                        }
                        relics.text = "遗物: " + string.Join(", ", names);
                    }
                    else
                    {
                        relics.text = "遗物: 无";
                    }
                }
            }
        }

        private void RefreshOrbs()
        {
            // Rebuild the visible row from the driver's orb pool via public API.
            var visible = driver.GetVisibleOrbs(8);

            if (handLabel != null)
            {
                handLabel.text = $"手牌 {driver.HandCount}/{OrbPool.HandLimit}";
            }

            for (int i = 0; i < orbCards.Count; i++)
            {
                OrbCardView view = orbCards[i];
                // UI slot 0 = rightmost; we fill from the right.
                int orbIndex = i; // card i is the i-th from the left; slot = 7 - i => rightmost is slot 0
                int slot = orbCards.Count - 1 - i;
                if (orbIndex < visible.Count)
                {
                    OrbInstance orb = visible[orbIndex];
                    view.gameObject.SetActive(true);
                    view.Setup(orb, slot, s => driver.ActionPlayOrb(s),
                        PickCardSprite(orb), PickIconSprite(orb), iconCorrupted);
                }
                else
                {
                    // Empty slot: show a blank card (disabled interaction).
                    view.gameObject.SetActive(true);
                    view.Setup(null, slot, s => driver.ActionPlayOrb(s), cardWhite, iconWhite, iconCorrupted);
                    view.SetDescription("(空)");
                }
            }
        }

        private Sprite PickCardSprite(OrbInstance orb)
        {
            if (orb.Locked)
            {
                return cardCorrupted;
            }
            switch (orb.Color)
            {
                case (int)OrbColor.Red: return cardRed;
                case (int)OrbColor.Yellow: return cardYellow;
                case (int)OrbColor.Blue: return cardBlue;
                case (int)OrbColor.White: return cardWhite;
                default: return cardWhite;
            }
        }

        private Sprite PickIconSprite(OrbInstance orb)
        {
            if (orb.Locked)
            {
                return iconCorrupted;
            }
            switch (orb.Color)
            {
                case (int)OrbColor.Red: return iconRed;
                case (int)OrbColor.Yellow: return iconYellow;
                case (int)OrbColor.Blue: return iconBlue;
                case (int)OrbColor.White: return iconWhite;
                default: return iconWhite;
            }
        }

        // ---------------- UI helpers ----------------

        private static GameObject LoadPrefabAtPath()
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/UI/Prefabs/OrbCard.prefab");
        }

        private static RectTransform CreateRect(Transform parent, string name, Vector2 anchor,
            Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return rt;
        }

        private TMP_Text CreateText(Transform parent, string name, string text, float size, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.font = font;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void PlaceCentered(TMP_Text text, Vector2 anchor, Vector2 size)
        {
            var rt = text.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
        }

        private Image CreateBar(Transform parent, string name, Color color, Vector2 anchor)
        {
            var rt = CreateRect(parent, name, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260, 14));
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Left;
            rt.gameObject.AddComponent<HudFillRef>().fill = img;
            return img;
        }

        private void CreateButton(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            var rt = CreateRect(parent, "Btn_" + label, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 44));
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.2f, 0.25f, 0.3f);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var text = CreateText(rt, "Label", label, 20, TextAlignmentOptions.Center);
            var textRt = text.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
        }
    }

    /// <summary>Helper component to hold a fill image reference by name.</summary>
    public class HudFillRef : MonoBehaviour
    {
        public Image fill;
    }
}
