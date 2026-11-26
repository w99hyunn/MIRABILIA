using UnityEngine;

namespace DoHwa.World
{
    // Scene-authored communal garden; crop progress uses the same saved state as player-built beds.
    public sealed class WorldGardenSite : MonoBehaviour
    {
        [SerializeField] private string persistentId;
        [SerializeField] private WorldBuilding building;

        public WorldBuilding Building => building;
        public string PersistentId => persistentId;
    }
}
