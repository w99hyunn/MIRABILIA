using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace DoHwa.World
{
    [Serializable]
    public sealed class MapVisibility
    {
        [SerializeField] private Renderer[] renderers = Array.Empty<Renderer>();
        [SerializeField] private Light[] lights = Array.Empty<Light>();
        [SerializeField] private Volume[] volumes = Array.Empty<Volume>();

        public void Apply(string scene, string current)
        {
            bool local = scene == current;
            bool surface = scene != "Moonshadow" && current != "Moonshadow";
            foreach (var renderer in renderers)
                renderer.forceRenderingOff = !(local || (surface && renderer is not ParticleSystemRenderer));
            bool interior = local && scene == "Moonshadow";
            foreach (var light in lights)
            {
                light.enabled = interior;
                if (interior && light.type == LightType.Directional) RenderSettings.sun = light;
            }
            foreach (var volume in volumes) volume.enabled = interior;
        }

        public static MapVisibility ForGeneratedChunk(GameObject root) => new MapVisibility
        {
            renderers = root.GetComponentsInChildren<Renderer>(true),
            lights = root.GetComponentsInChildren<Light>(true),
            volumes = root.GetComponentsInChildren<Volume>(true)
        };
    }
}
