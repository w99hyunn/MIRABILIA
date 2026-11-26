using System;
using System.Collections.Generic;
using DoHwa.World;
using TMPro;
using UnityEngine;

namespace DoHwa.UI
{
    public sealed class WorldSaveListView : MonoBehaviour
    {
        [SerializeField] private WorldSaveRow rowPrefab;
        [SerializeField] private TMP_Text empty;
        private readonly List<WorldSaveRow> rows = new();

        public void Render(IReadOnlyList<WorldSlot> saves, Action<WorldSlot> select, Action<WorldSlot, Vector2> context)
        {
            for (int i = 0; i < saves.Count; i++)
            {
                if (i == rows.Count) rows.Add(Instantiate(rowPrefab, transform));
                var row = rows[i];
                row.Bind(saves[i], select, context);
                row.gameObject.SetActive(true);
            }
            for (int i = saves.Count; i < rows.Count; i++) rows[i].gameObject.SetActive(false);
            empty.gameObject.SetActive(saves.Count == 0);
        }

        public void Select(string slot)
        {
            foreach (var row in rows) row.Select(row.Slot == slot);
        }

        public void SetInteractable(bool enabled)
        {
            foreach (var row in rows) row.SetInteractable(enabled);
        }
    }
}
