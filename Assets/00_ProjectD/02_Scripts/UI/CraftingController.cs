using System;
using DoHwa.World;

namespace DoHwa.UI
{
    internal sealed class CraftingController
    {
        private readonly WorldRuntime world;
        public CraftingController(WorldRuntime world) => this.world = world;
        public bool HasRecipe(int item) => RecipeFor(item) != null;
        public int FirstResult => world.Catalog.recipes.Length == 0 ? -1 : world.Catalog.recipes[0].result.item;
        private CraftRecipe RecipeFor(int item) => Array.Find(world.Catalog.recipes, recipe => recipe.result.item == item);

        public void Show(int item, InventoryItem[] items, ItemDetailView detail)
        {
            var recipe = RecipeFor(item);
            detail.ShowRecipe(recipe, items, world.HasStation(world.LocalPlayer, recipe.station), world.MaxCraftCount(recipe));
        }

        public bool Submit(int item, bool makeMaximum)
        {
            var recipe = RecipeFor(item);
            int quantity = makeMaximum ? world.MaxCraftCount(recipe) : 1;
            if (quantity < 1) return false;
            world.Submit(new WorldCommand { action = WorldAction.Craft, item = item, target = recipe.id, quantity = quantity });
            return true;
        }
    }
}
