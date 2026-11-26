using System;
using TMPro;
using UnityEngine;
using DoHwa.World;

namespace DoHwa.UI
{
    public sealed class VitalsController : MonoBehaviour
    {
        [Serializable]
        private sealed class GaugeView
        {
            [SerializeField] private TMP_Text label;
            [SerializeField] private UiGaugeMotion motion;

            public void Render(float value)
            {
                label.text = $"{value:0} / 100";
                motion.SetValue(value / 100f);
            }

            public void Pulse() => motion.Pulse();
        }

        [SerializeField] private WorldRuntime world;
        [SerializeField] private GaugeView health;
        [SerializeField] private GaugeView food;
        [SerializeField] private EquipmentPanel equipment;

        private void OnEnable()
        {
            world.Changed += Render;
            Render();
        }

        private void OnDisable() => world.Changed -= Render;

        private void Render()
        {
            var player = world.LocalPlayer;
            if (player == null) return;
            health.Render(player.health);
            food.Render(player.food);
            equipment.RefreshStats(player.health, player.food);
        }

        public void Pulse()
        {
            health.Pulse();
            food.Pulse();
        }
    }
}
