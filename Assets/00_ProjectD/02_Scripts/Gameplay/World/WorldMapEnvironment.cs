using UnityEngine;

namespace DoHwa.World
{
    public sealed class WorldMapEnvironment : MonoBehaviour
    {
        [SerializeField] private GameObject blossomWeather;
        [SerializeField] private DokkaebiGate[] gates = System.Array.Empty<DokkaebiGate>();
        [SerializeField] private WorldEnemy[] enemies = System.Array.Empty<WorldEnemy>();
        [SerializeField] private WorldSupplyCache[] caches = System.Array.Empty<WorldSupplyCache>();
        [SerializeField] private WorldResource[] resources = System.Array.Empty<WorldResource>();
        [SerializeField] private WorldGardenSite[] gardens = System.Array.Empty<WorldGardenSite>();

        [SerializeField] private SeededResourceRegion[] seededRegions = System.Array.Empty<SeededResourceRegion>();

        public void Render(WorldState state)
        {
            foreach (var gate in gates) gate.Render(state);
            foreach (var cache in caches) cache.Render(state);
            foreach (var region in seededRegions) region.ApplySeed(state.seed);
        }

        [SerializeField] private MapVisibility visibility = new();
        [SerializeField] private OpenWorldStreamer[] streamers = System.Array.Empty<OpenWorldStreamer>();

        public void SetVisibleMap(string current)
        {
            visibility.Apply(gameObject.scene.name, current);
            foreach (var streamer in streamers) streamer.SetVisibleMap(current);
        }

        public DokkaebiGate[] Gates => gates;
        public WorldEnemy[] Enemies => enemies;
        public WorldSupplyCache[] Caches => caches;
        public WorldResource[] Resources => resources;
        public WorldGardenSite[] Gardens => gardens;

        [SerializeField] private BlossomDrift blossomDrift;
        private static readonly System.Collections.Generic.HashSet<WorldMapEnvironment> active = new();
        private static event System.Action<WorldMapEnvironment> activated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetActive()
        {
            active.Clear();
            activated = null;
        }

        internal static void Subscribe(System.Action<WorldMapEnvironment> register)
        {
            activated += register;
            foreach (var environment in active) register(environment);
        }

        internal static void Unsubscribe(System.Action<WorldMapEnvironment> register) => activated -= register;

        internal void Bind(WorldRuntime world)
        {
            foreach (var enemy in enemies) enemy.Bind(world);
            foreach (var streamer in streamers) streamer.Bind(world);
            if (blossomDrift) blossomDrift.Bind(world.Hero);
        }

        private void Awake()
        {
            active.Add(this);
            activated?.Invoke(this);
        }
        private void OnDestroy() => active.Remove(this);

        public void SetBlossoms(bool visible) { if(blossomWeather) blossomWeather.SetActive(visible); }
    }
}
