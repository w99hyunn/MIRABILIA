using UnityEngine;

namespace DoHwa.World
{
    public sealed class WorldBuilding : MonoBehaviour
    {
        [SerializeField] private MapVisibility visibility = new();
        [SerializeField] private GardenPresentation garden;
        public void SetVisibleMap(string current) => visibility.Apply(gameObject.scene.name, current);

        public string InstanceId { get; private set; }
        public BuildingSpec Spec { get; private set; }
        public void Bind(WorldRuntime world, PlacedBuilding state, BuildingSpec spec)
        {
            InstanceId = state.id;
            Spec = spec;
            if (garden) garden.Bind(world, this);
            SetVisibleMap(world.LocalPlayer.mapScene);
        }
    }
}
