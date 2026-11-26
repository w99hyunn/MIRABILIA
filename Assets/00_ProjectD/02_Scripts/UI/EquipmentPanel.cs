using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace DoHwa.UI
{
    public enum EquipmentPart { None=-1, Weapon, Head, Body, Legs, Feet, Necklace, Ring, Earrings, Bag }

    public sealed class EquipmentPanel : MonoBehaviour
    {
        public static readonly string[] PartNames={"무기","모자","상의","하의","신발","목걸이","반지","귀걸이","가방"};
        public event System.Action<int> PartSelected;
        [SerializeField] private ItemDetailView equippedItem;
        [SerializeField] private GameObject equipmentQuantityRow;
        [SerializeField] private TMP_Text equipmentLabel;
        [SerializeField] private UnityEngine.UI.Image equipmentIcon;
        [SerializeField] private Image[] icons;
        [SerializeField] private TMP_Text[] names;
        [SerializeField] private EquipmentSlotFeedback[] slotFeedback;
        [SerializeField] private TMP_Text[] statValues;

        public void RefreshStats(float health, float food)
        {
            statValues[0].text = $"{health:0}<size=16> / 100</size>";
            statValues[1].text = $"{food:0}<size=16> / 100</size>";
        }
        public void SelectPart(int part) => PartSelected?.Invoke(part);
        public void PulsePart(int part) => slotFeedback[part].Pulse();
        public void ShowReason(string reason) => equippedItem.ShowReason(reason);

        public void ShowWeapon(InventoryItem[] items, int weapon)
        {
            equipmentLabel.text = "무기\n" + (weapon >= 0 ? items[weapon].title : "미착용");
            equipmentIcon.sprite = weapon >= 0 ? items[weapon].icon : null;
            equipmentIcon.enabled = weapon >= 0;
        }

        public void Render(InventoryItem[] items, int[] loadout, int part, int selected)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                int id = loadout[i];
                bool filled = id >= 0;
                icons[i].enabled = filled;
                icons[i].sprite = filled ? items[id].icon : null;
                names[i].text = filled ? items[id].title : "미착용";
                slotFeedback[i].SetState(i == part, filled);
            }
            int equipped = loadout[part];
            bool candidate = selected >= 0 && items[selected].equipmentPart == (EquipmentPart)part && selected != equipped;
            int shown = candidate ? selected : equipped;
            equipmentQuantityRow.SetActive(shown >= 0);
            equippedItem.ShowEquipment(PartNames[part], shown >= 0 ? items[shown] : null,
                shown >= 0 && System.Array.IndexOf(loadout, shown) >= 0);
        }
    }
}
