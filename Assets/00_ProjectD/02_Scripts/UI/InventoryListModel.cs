using System;
using System.Linq;

namespace DoHwa.UI
{
    internal sealed class InventoryListModel
    {
        public int Filter { get; set; }
        public int SortOrder { get; set; }

        public int[] VisibleItems(InventoryItem[] items, Func<int, bool> includeRecipe = null)
        {
            var indices = Enumerable.Range(0, items.Length);
            if (includeRecipe != null) indices = indices.Where(includeRecipe);
            else if (Filter != 0)
            {
                string category = Filter switch { 1 => "장비", 2 => "소모품", _ => "재료" };
                indices = indices.Where(index => items[index].category == category);
            }
            return (SortOrder switch
            {
                1 => indices.OrderBy(index => items[index].category, StringComparer.CurrentCulture)
                            .ThenBy(index => items[index].title, StringComparer.CurrentCulture),
                2 => indices.OrderBy(index => items[index].title, StringComparer.CurrentCulture),
                3 => indices.OrderByDescending(index => items[index].quantity).ThenBy(index => index),
                _ => indices
            }).ToArray();
        }
    }
}
