using System;
using PunishingTower.SignalOrb;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PunishingTower.UI
{
    /// <summary>
    /// Signal orb card view (Slay the Spire style vertical card).
    /// Top area: orb icon. Bottom area: reserved for affixes/description text.
    /// Clicking the card triggers the orb play callback with its UI slot.
    /// </summary>
    public class OrbCardView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image orbIcon;
        [SerializeField] private Image lockOverlay;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text footerText;

        private OrbInstance orb;
        private int slot;
        private Action<int> onClick;

        public int Slot => slot;

        public void Setup(OrbInstance orbInstance, int uiSlot, Action<int> clickHandler,
            Sprite backgroundSprite, Sprite iconSprite, Sprite lockedIconSprite)
        {
            orb = orbInstance;
            slot = uiSlot;
            onClick = clickHandler;

            if (cardBackground != null)
            {
                cardBackground.sprite = backgroundSprite;
            }
            if (orbIcon != null)
            {
                orbIcon.sprite = iconSprite;
            }
            if (lockOverlay != null)
            {
                lockOverlay.sprite = lockedIconSprite;
            }

            Refresh();
        }

        public void SetDescription(string text)
        {
            if (descriptionText != null)
            {
                descriptionText.text = text;
            }
        }

        /// <summary>Refreshes visuals from the orb state (locked flag, footer).</summary>
        public void Refresh()
        {
            if (orb == null)
            {
                return;
            }

            if (lockOverlay != null)
            {
                lockOverlay.gameObject.SetActive(orb.Locked);
            }

            if (footerText != null)
            {
                string colorName;
                switch (orb.Color)
                {
                    case (int)PunishingTower.Data.OrbColor.Red: colorName = "红"; break;
                    case (int)PunishingTower.Data.OrbColor.Yellow: colorName = "黄"; break;
                    case (int)PunishingTower.Data.OrbColor.Blue: colorName = "蓝"; break;
                    case (int)PunishingTower.Data.OrbColor.White: colorName = "白"; break;
                    default: colorName = "?"; break;
                }
                footerText.text = orb.Locked ? $"[{colorName}] 腐化" : $"[{colorName}]";
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null && orb != null)
            {
                onClick.Invoke(slot);
            }
        }
    }
}
