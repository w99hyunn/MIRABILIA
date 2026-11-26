using UnityEngine;

namespace DoHwa.World
{
    public sealed class WorldSupplyCache : MonoBehaviour
    {
        [SerializeField] private string cacheId;
        [SerializeField] private SupplyCacheDefinition definition;
        [SerializeField] private Animator animator;
        public string Id => cacheId;
        public SupplyCacheDefinition Definition => definition;
        public bool Opened { get; private set; }
        public bool Unlocked { get; private set; }

        public void Render(WorldState state)
        {
            Opened = state.openedCaches.Contains(cacheId);
            Unlocked = string.IsNullOrEmpty(definition.requiredBoss) || state.defeatedBosses.Contains(definition.requiredBoss);
            animator.SetBool("Opened", Opened);
        }
    }
}


