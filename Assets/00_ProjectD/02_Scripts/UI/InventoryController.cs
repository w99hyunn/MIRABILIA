using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DoHwa.UI
{
    public sealed class InventoryController : MonoBehaviour
    {
        [SerializeField] private DoHwa.World.ItemCatalog itemCatalog;
        [SerializeField] private ItemDetailView bagItem;
        [SerializeField] private GameObject inventory;
        [SerializeField] private GameObject character;
        [SerializeField] private GameObject bagDetail;
        [SerializeField] private GameObject craftingStage;
        [SerializeField] private EquipmentPanel equipmentPanel;
        [SerializeField] private UnityEngine.UI.Image craftingPreview;
        [SerializeField] private CraftResultDialog craftResult;
        [SerializeField] private VitalsController vitals;
        [SerializeField] private DoHwa.World.WorldRuntime world;
        [SerializeField] private QuickSlotBar quickBar;
        [SerializeField] private InventorySortMenu sortMenu;
        [SerializeField] private InventoryView inventoryView;
        private readonly InventoryListModel inventoryList = new();
        private readonly int[] loadout = new int[EquipmentPanel.PartNames.Length];
        private InventoryItem[] items;
        private CraftingController recipes;
        private int selected, selectedPart;
        private enum Page { Inventory, Crafting, Equipment }
        private Page page;
        private bool equipmentMode => page == Page.Equipment;
        private bool crafting => page == Page.Crafting;
        public bool IsCrafting => crafting;

        private void Awake()
        {
            Array.Fill(loadout, -1);
            recipes = new CraftingController(world);
            items = CreateItemViews();
            Refresh();
        }
        private void OnEnable()
        {
            foreach (var slot in inventoryView.Slots) slot.Selected += Select;
            foreach (var slot in quickBar.Slots) slot.Selected += UseQuick;
            equipmentPanel.PartSelected += SelectEquipmentPart;
            sortMenu.SortSelected += SetSort;
            world.Changed += RefreshPlayer;
            world.Result += ShowWorldResult;
            RefreshPlayer();
        }
        private void OnDisable()
        {
            foreach (var slot in inventoryView.Slots) slot.Selected -= Select;
            foreach (var slot in quickBar.Slots) slot.Selected -= UseQuick;
            equipmentPanel.PartSelected -= SelectEquipmentPart;
            sortMenu.SortSelected -= SetSort;
            world.Changed -= RefreshPlayer;
            world.Result -= ShowWorldResult;
        }
        private void RefreshPlayer()
        {
            var player = world.LocalPlayer;
            if (player != null) ApplyWorldState(player);
        }

        public void ApplyWorldState(DoHwa.World.SurvivorState player)
        {
            for (int i = 0; i < items.Length; i++) items[i].quantity = player.items[i];
            Array.Copy(player.equipment, loadout, loadout.Length);
            Refresh();
        }

        public void ShowWorldResult(DoHwa.World.WorldCommand command, bool success, string reason)
        {
            if (!success) { ShowCondition(reason); return; }
            MarkItemChanged();
            if (command.action == DoHwa.World.WorldAction.Craft)
            {
                var recipe = world.Catalog.Recipe(command.target);
                craftResult.Show(items[recipe.result.item].icon, items[recipe.result.item].title,
                    recipe.result.amount * Math.Max(1, command.quantity));
            }
        }
        private bool IsEquipped(int id) => Array.IndexOf(loadout,id)>=0;

        private InventoryItem[] CreateItemViews() => itemCatalog.items.Select(item => new InventoryItem
        {
            title = item.title, category = item.category, description = item.description,
            icon = item.icon, equipmentPart = item.equipmentPart
        }).ToArray();

        public void PrepareInventory()
        {
            page = Page.Inventory;
            inventoryList.Filter = selected = 0;
            bagDetail.SetActive(false);
            Refresh();
            inventoryView.ResetScroll();
        }
        public void PrepareCrafting()
        {
            page = Page.Crafting;
            inventoryList.Filter = 0;
            if (recipes.FirstResult >= 0) Select(recipes.FirstResult);
            else Refresh();
            inventoryView.ResetScroll();
        }

        public void PrepareEquipment()
        {
            page = Page.Equipment;
            inventoryList.Filter = 0;
            selectedPart = (int)EquipmentPart.Weapon;
            selected = loadout[selectedPart] >= 0 ? loadout[selectedPart] : 0;
            Refresh();
        }

        private void SelectEquipmentPart(int part)
        {
            bool changed = selectedPart != part;
            selectedPart = part;
            if (loadout[part] >= 0) selected = loadout[part];
            Refresh();
            if (changed && character.TryGetComponent<UiPanelReveal>(out var reveal)) reveal.RefreshContent(2);
        }

        public void SetFilter(int value)
        {
            bool changed = inventoryList.Filter != value || crafting;
            page = Page.Inventory;
            inventoryList.Filter = value;
            if (changed) bagDetail.SetActive(false);
            Refresh();
            inventoryView.ResetScroll();
            if (changed && inventory.TryGetComponent<UiPanelReveal>(out var reveal)) reveal.RefreshContent(1);
        }
        public void Select(int index)
        {
            if (index < 0 || (equipmentMode && items[index].equipmentPart == EquipmentPart.None)) return;
            bool changed = selected != index || (!equipmentMode && !bagDetail.activeSelf);
            selected = index;
            if (equipmentMode) selectedPart = (int)items[index].equipmentPart;
            else bagDetail.SetActive(true);
            Refresh();
            var panel = equipmentMode ? character : inventory;
            if (changed && panel.TryGetComponent<UiPanelReveal>(out var reveal)) reveal.RefreshContent(2);
        }
        public void Sort() => sortMenu.Toggle();
        public void SetSort(int order) { inventoryList.SortOrder=order; Refresh(); inventoryView.ResetScroll(); }

        public void UseQuick(int index)
        {
            page = Page.Inventory;
            selected = quickBar.Select(index);
            var item = items[selected];
            if (item.category == "소모품" || (item.equipmentPart == EquipmentPart.Weapon && !IsEquipped(selected)))
            {
                UseSelected();
                return;
            }
            Refresh();
            MarkItemChanged();
        }

        private void MarkItemChanged()
        {
            inventoryView.Pulse(selected);
            quickBar.Pulse(selected);
            if(equipmentMode) equipmentPanel.PulsePart(selectedPart);
        }

        private void ShowCondition(string reason)
        {
            Refresh();
            if (inventory.activeSelf || character.activeSelf)
            {
                if (equipmentMode) equipmentPanel.ShowReason(reason);
                else bagItem.ShowReason(reason);
            }
            else vitals.Pulse();
        }
        public void UseSelected()
        {
            if(craftResult.IsOpen || selected < 0) return;
            var command = new DoHwa.World.WorldCommand { item = selected };
            if (crafting)
            {
                bool maximum = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
                if (!recipes.Submit(selected, maximum)) ShowCondition("재료와 제작 시설을 확인하세요");
                return;
            }
            else if (items[selected].equipmentPart != EquipmentPart.None)
            { command.action = DoHwa.World.WorldAction.Equip; command.part = (int)items[selected].equipmentPart; }
            else command.action = DoHwa.World.WorldAction.Consume;
            world.Submit(command);
        }

        public void Refresh()
        {
            var detail = bagItem;
            var visible = inventoryList.VisibleItems(items, crafting ? recipes.HasRecipe : null);
            if (!visible.Contains(selected)) selected = visible.Length > 0 ? visible[0] : -1;
            inventoryView.Render(items, visible, selected, loadout, bagDetail.activeSelf, inventoryList.Filter, crafting, equipmentMode);
            quickBar.Render(items);
            var item = selected >= 0 ? items[selected] : null;
            craftingStage.SetActive(crafting && item != null);
            if (item == null) bagDetail.SetActive(false);
            else if (crafting)
            {
                craftingPreview.sprite = item.icon;
                recipes.Show(selected, items, detail);
            }
            else if (!equipmentMode) detail.ShowItem(item, IsEquipped(selected));
            equipmentPanel.ShowWeapon(items, loadout[0]);
            equipmentPanel.gameObject.SetActive(equipmentMode);
            if (equipmentMode) equipmentPanel.Render(items, loadout, selectedPart, selected);
        }
    }
}
