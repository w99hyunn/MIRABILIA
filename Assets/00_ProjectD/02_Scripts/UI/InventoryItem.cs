using System;
using UnityEngine;
namespace DoHwa.UI
{
        [Serializable]
        public sealed class InventoryItem
        {
            public string title, category, description;
            public Sprite icon;
            public int quantity;
            public EquipmentPart equipmentPart=EquipmentPart.None;
        }

}
