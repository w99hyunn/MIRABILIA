using UnityEngine;

namespace DoHwa
{
    public sealed class BlossomDrift : MonoBehaviour
    {
        private Transform target;
        private System.Threading.CancellationTokenRegistration heroLifetime;
        [SerializeField] private float height = 6f;

        internal void Bind(HeroMotor hero)
        {
            target = hero.transform;
            heroLifetime = hero.destroyCancellationToken.Register(() => enabled = false);
            enabled = true;
        }
        private void Start()
        {
            if (!target) enabled = false;
        }
        private void OnDestroy() => heroLifetime.Dispose();

        private void LateUpdate()
        {
            // World-space particles keep their flight path when the emitter follows the hero.
            transform.position = target.position + Vector3.up * height;
        }
    }
}
