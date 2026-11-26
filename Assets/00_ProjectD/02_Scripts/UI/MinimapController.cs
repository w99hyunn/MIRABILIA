using UnityEngine;

namespace DoHwa.UI
{
    public sealed class MinimapController : MonoBehaviour
    {
        [SerializeField] private HeroMotor hero;
        [SerializeField] private Camera mapCamera;
        [SerializeField] private GameObject expandedMap;
        [SerializeField] private RectTransform marker;

        public void Configure(HeroMotor actor, Camera camera)
        {
            hero = actor;
            mapCamera = camera;
        }

        private void LateUpdate()
        {
            bool expanded = expandedMap.activeSelf;
            var cameraTransform = mapCamera.transform;
            cameraTransform.position = hero.transform.position + Vector3.up * (expanded ? 90f : 45f);
            mapCamera.orthographicSize = expanded ? 35f : 14f;
            float x = Vector3.Dot(hero.Facing, cameraTransform.right);
            float y = Vector3.Dot(hero.Facing, cameraTransform.up);
            marker.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Atan2(x, y) * Mathf.Rad2Deg);
        }
    }
}
