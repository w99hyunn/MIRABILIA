using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DoHwa.UI
{
    public sealed class QuickSlotBar : MonoBehaviour
    {
        [SerializeField] private GameObject quickTooltip;
        [SerializeField] private TMP_Text tooltipLabel;
        [SerializeField] private RpgItemButton[] quickSlots;
        [SerializeField] private GameObject quickMenu;
        [SerializeField] private UnityEngine.UI.Image activeTool;
        [SerializeField] private TMP_Text[] quickNames;
        [SerializeField] private int[] quickItems;
        private int selected;
        private InventoryItem[] displayedItems;
        public System.Collections.Generic.IReadOnlyList<RpgItemButton> Slots => quickSlots;

        private void OnEnable()
        {
            foreach (var slot in quickSlots)
            {
                slot.Hovered += ShowTooltip;
                slot.PointerLeft += HideTooltip;
            }
        }
        private void OnDisable()
        {
            foreach (var slot in quickSlots)
            {
                slot.Hovered -= ShowTooltip;
                slot.PointerLeft -= HideTooltip;
            }
        }

        public bool IsOpen => quickMenu.activeSelf;
        public int Count => quickItems.Length;
        public void HideTooltip() => quickTooltip.SetActive(false);

        private void ShowTooltip(int index)
        {
            var item = displayedItems[quickItems[index]];
            quickTooltip.SetActive(true);
            tooltipLabel.text = $"{item.title}   /   {item.quantity}개";
        }
        public void Close()
        {
            HideTooltip();
            quickMenu.SetActive(false);
        }
        public void Toggle()
        {
            HideTooltip();
            quickMenu.SetActive(!quickMenu.activeSelf);
        }
        public int Select(int index)
        {
            Close();
            selected = index;
            return quickItems[index];
        }
        public void Pulse(int item)
        {
            int index = Array.IndexOf(quickItems, item);
            foreach (var slot in quickSlots) slot.PulseItem(index);
        }
        public void Render(InventoryItem[] items)
        {
            displayedItems = items;
            for (int i = 0; i < quickSlots.Length; i++)
            {
                var item = items[quickItems[i]];
                quickNames[i].text = item.title;
                quickSlots[i].Show(i, item.icon, item.quantity, i == selected, false);
            }
            activeTool.sprite = items[quickItems[selected]].icon;
        }
    }
}
