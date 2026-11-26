using System;
using System.Linq;
using DoHwa.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoHwa.UI
{
    [Serializable]
    public sealed class ItemDetailView
    {
        [SerializeField] private TMP_Text title, category, description, count, actionLabel;
        [SerializeField] private Button action;
        [SerializeField] private Image detailArt;

        public void ShowItem(InventoryItem item, bool equipped)
        {
            title.text = item.title;
            category.text = item.category;
            description.text = item.description;
            count.text = $"보유 수량<pos=240>{item.quantity}";
            detailArt.enabled = true;
            detailArt.sprite = item.icon;
            detailArt.preserveAspect = true;
            bool gear = item.equipmentPart != EquipmentPart.None;
            SetAction(gear ? (equipped ? "장착 중" : "장착하기") : item.category == "소모품" ? "사용하기" : "제작 재료",
                item.quantity > 0 && (gear ? !equipped : item.category == "소모품"));
        }

        public void ShowEquipment(string part, InventoryItem item, bool equipped)
        {
            if (item == null)
            {
                title.text = part;
                category.text = "장비 부위";
                description.text = "착용한 장비가 없습니다.";
                count.text = "";
                detailArt.enabled = false;
                SetAction("장착할 장비 없음", false);
                return;
            }
            ShowItem(item, equipped);
            category.text = "장비 / " + part;
            SetAction(equipped ? "장착 해제" : "장착하기", item.quantity > 0);
        }

        public void ShowRecipe(CraftRecipe recipe, InventoryItem[] items, bool hasStation, int maximum)
        {
            var item = items[recipe.result.item];
            title.text = item.title;
            detailArt.enabled = false;
            count.text = string.Join("\n", recipe.ingredients.Select(cost =>
                items[cost.item].title + "<pos=240>" + items[cost.item].quantity + " / " + cost.amount));
            string station = recipe.station switch
            {
                StationKind.Workbench => "작업대", StationKind.Furnace => "용광로",
                StationKind.Forge => "대장간", StationKind.Alchemy => "연금대", _ => "손 제작"
            };
            category.text = station + (hasStation ? "" : " 필요");
            description.text = item.description + $"\n\n한 번에 {recipe.result.amount}개 제작\n최대 {maximum}회 제작 가능";
            if (maximum > 1) description.text += "\nShift + 클릭으로 최대 제작";
            SetAction(!hasStation ? "시설 필요" : maximum > 0 ? "제작하기" : "제작 불가", maximum > 0);
        }

        public void ShowReason(string reason) => count.text += "\n<color=#AE5147>" + reason + "</color>";

        private void SetAction(string label, bool available)
        {
            actionLabel.text = label;
            action.interactable = available;
        }
    }
}
