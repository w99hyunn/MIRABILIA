using TMPro;
using UnityEngine;

namespace DoHwa.UI
{
    public sealed class InventoryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text inventoryHeader;
        [SerializeField] private TMP_Text inventoryHeading;
        [SerializeField] private TMP_Text inventorySubheading;
        [SerializeField] private UiVisualState[] filters;
        [SerializeField] private UiVisualState craftFilter;
        [SerializeField] private GameObject inventorySort;
        [SerializeField] private GameObject inventoryEquipment;
        [SerializeField] private UnityEngine.UI.ScrollRect inventoryScroll;
        [SerializeField] private UiVisualState equipmentTab;
        [SerializeField] private RpgItemButton[] slots;

        public System.Collections.Generic.IReadOnlyList<RpgItemButton> Slots => slots;

        public void ResetScroll()
        {
            inventoryScroll.StopMovement();
            inventoryScroll.verticalNormalizedPosition = 1;
        }

        public void Pulse(int item)
        {
            foreach (var slot in slots) slot.PulseItem(item);
        }

        public void Render(InventoryItem[] items, int[] visible, int selected, int[] loadout,
            bool detailOpen, int filter, bool crafting, bool equipment)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                int id = i < visible.Length ? visible[i] : -1;
                var item = id >= 0 ? items[id] : null;
                slots[i].Show(id, item?.icon, item?.quantity ?? 0, id == selected && detailOpen,
                    id >= 0 && System.Array.IndexOf(loadout, id) >= 0, !crafting && filter == 0);
            }
            inventoryHeader.text = equipment ? "장비" : crafting ? "제작" : "배낭";
            inventoryHeading.text = equipment ? "부위별 장비" : crafting ? "제작법" : "소지품";
            inventorySubheading.text = equipment ? "부위를 선택하거나 아래 소지 장비를 선택하세요" :
                crafting ? "숲에서 모은 재료로 여정을 준비하세요" : "여정에서 모은 도구와 작은 발견들";
            for (int i = 0; i < filters.Length; i++)
                filters[i].SetActiveState(!equipment && !crafting && filter == i);
            craftFilter.SetActiveState(crafting);
            inventoryScroll.gameObject.SetActive(!equipment);
            inventorySort.SetActive(!equipment);
            inventoryEquipment.SetActive(false);
            equipmentTab.SetActiveState(equipment);
        }
    }
}
