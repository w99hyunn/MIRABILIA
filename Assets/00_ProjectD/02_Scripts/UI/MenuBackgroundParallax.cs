using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DoHwa.UI
{
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public sealed class MenuBackgroundParallax : MonoBehaviour
    {
        [SerializeField] private float distance = 12f;
        [SerializeField] private Texture2D sky;
        [SerializeField] private Texture2D island;
        [SerializeField] private Material islandWaterMaterial;
        [SerializeField] private Texture2D leftPeach;
        [SerializeField] private Texture2D centerPeach;
        [SerializeField] private Texture2D rightPeach;
        [SerializeField] private Texture2D windowLight;
        [SerializeField] private Texture2D star;

        private static readonly Vector2 ArtworkSize = new(1672f, 941f);
        private static readonly Vector2[] StarPositions =
        {
            new(113, 326), new(219, 264), new(330, 125), new(357, 343),
            new(189, 598), new(306, 657), new(472, 159), new(474, 736),
            new(1285, 67), new(1452, 191), new(1364, 353), new(1573, 371),
            new(1211, 476), new(1329, 576), new(1506, 627), new(1188, 701),
            new(944, 849), new(606, 836)
        };

        private Image artwork;
        private RectTransform rect;
        private RectTransform layers;
        private RectTransform stars;
        private RectTransform islandMotion;
        private RectTransform leftPeachMotion;
        private RectTransform centerPeachMotion;
        private RectTransform rightPeachMotion;
        private RawImage window;
        private RawImage[] starImages;
        private Camera canvasCamera;
        private Vector2 origin;
        private Vector3 originalScale;
        private Vector2 pointerOffset;
        private float sway;
        private float tap;
        private float tapAge;
        private float windowResponse;

        private void Awake()
        {
            TryGetComponent<RectTransform>(out rect);
            TryGetComponent<Image>(out artwork);
            canvasCamera = artwork.canvas.worldCamera;
            origin = rect.anchoredPosition;
            originalScale = rect.localScale;

            layers = AddGroup("Orchard layers", rect);
            RawImage skyImage = AddLayer("Sky", layers, sky, Vector2.zero, ArtworkSize, new Vector2(0.5f, 0.5f));
            skyImage.material = artwork.material;

            stars = AddGroup("Twinkling stars", layers);
            starImages = new RawImage[StarPositions.Length];
            for (int index = 0; index < starImages.Length; index++)
            {
                float size = 10f + index % 5 * 3f;
                Vector2 sizeInArtwork = Vector2.one * size;
                starImages[index] = AddLayer("Star", stars, star, StarPositions[index] - sizeInArtwork * 0.5f,
                    sizeInArtwork, new Vector2(0.5f, 0.5f));
                starImages[index].color = Color.clear;
            }

            islandMotion = AddGroup("Floating island", layers);
            RawImage islandImage = AddLayer("Island", islandMotion, island, new Vector2(288, -80),
                new Vector2(1072, 976), new Vector2(0.5f, 0.5f));
            islandImage.material = islandWaterMaterial;
            leftPeachMotion = AddLayer("Left peach", islandMotion, leftPeach, new Vector2(666, 128),
                new Vector2(110, 131), new Vector2(0.5f, 0.78f)).rectTransform;
            centerPeachMotion = AddLayer("Center peach", islandMotion, centerPeach, new Vector2(929, 113),
                new Vector2(161, 162), new Vector2(0.60f, 0.79f)).rectTransform;
            rightPeachMotion = AddLayer("Right peach", islandMotion, rightPeach, new Vector2(1083, 284),
                new Vector2(107, 125), new Vector2(0.36f, 0.83f)).rectTransform;
            window = AddLayer("Window light", islandMotion, windowLight, new Vector2(543, 370),
                new Vector2(173, 107), new Vector2(0.5f, 0.5f));
        }

        private void OnEnable()
        {
            artwork.enabled = false;
            layers.gameObject.SetActive(true);
            rect.localScale = originalScale * 1.045f;
            UpdateMotion(Vector2.zero, Vector2.zero, false, false, 0f);
        }

        private void OnDisable()
        {
            layers.gameObject.SetActive(false);
            artwork.enabled = true;
            rect.anchoredPosition = origin;
            rect.localScale = originalScale;
        }

        private void LateUpdate()
        {
            Vector2 target = Vector2.zero;
            Vector2 artworkPosition = Vector2.zero;
            bool pressed = false;
            bool hasPointer = Application.isFocused && ReadPointer(out target, out pressed);
            if (hasPointer)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(layers, target, canvasCamera, out Vector2 local))
                {
                    Rect bounds = layers.rect;
                    artworkPosition = new Vector2(
                        (local.x - bounds.xMin) / Mathf.Max(1f, bounds.width) * ArtworkSize.x,
                        (bounds.yMax - local.y) / Mathf.Max(1f, bounds.height) * ArtworkSize.y);
                }
                target = new Vector2(
                    Mathf.Clamp(target.x / Screen.width * 2f - 1f, -1f, 1f),
                    Mathf.Clamp(target.y / Screen.height * 2f - 1f, -1f, 1f));
            }

            UpdateMotion(target, artworkPosition, hasPointer, pressed, Time.unscaledDeltaTime);
        }

        private void UpdateMotion(Vector2 target, Vector2 pointer, bool hasPointer, bool pressed, float deltaTime)
        {
            float smoothing = 1f - Mathf.Exp(-5f * deltaTime);
            Vector2 previousOffset = pointerOffset;
            pointerOffset = Vector2.Lerp(pointerOffset, target, smoothing);
            float scale = rect.rect.height / ArtworkSize.y;
            float time = Time.unscaledTime;
            rect.anchoredPosition = origin + pointerOffset * (distance * 0.55f * scale);
            stars.anchoredPosition = pointerOffset * (distance * 0.32f * scale);
            islandMotion.anchoredPosition = (pointerOffset * (distance * 1.45f) +
                new Vector2(1.7f * Mathf.Sin(time * 0.64f), 5f * Mathf.Sin(time * 0.92f))) * scale;

            Vector2 islandPointer = pointer - new Vector2(islandMotion.anchoredPosition.x, -islandMotion.anchoredPosition.y) /
                Mathf.Max(0.001f, scale);
            float treeProximity = hasPointer ? Proximity(islandPointer, new Vector2(976, 223), 320f) : 0f;
            float houseProximity = hasPointer ? Proximity(islandPointer, new Vector2(625, 423), 235f) : 0f;
            if (pressed && hasPointer && Proximity(islandPointer, new Vector2(825, 398), 425f) > 0f)
            {
                tap = 1f;
                tapAge = 0f;
            }

            tap *= Mathf.Exp(-2.3f * deltaTime);
            tapAge += deltaTime;
            float pointerMotion = (pointerOffset.x - previousOffset.x) / Mathf.Max(0.001f, deltaTime) * 1.8f;
            sway = Mathf.Lerp(sway, pointerMotion + pointerOffset.x * treeProximity * 4f, smoothing);
            float response = Mathf.Clamp(sway, -8f, 8f);
            float swing = tap * 8f * Mathf.Sin(tapAge * 10f);
            float leftAngle = 2.5f * Mathf.Sin(time * 1.74f + 0.3f) + response + swing;
            float centerAngle = 2.1f * Mathf.Sin(time * 1.43f + 1.8f) + response * 0.72f - swing * 0.65f;
            float rightAngle = 3.4f * Mathf.Sin(time * 1.96f + 3.1f) + response * 1.15f + swing * 0.85f;
            leftPeachMotion.localRotation = Quaternion.Euler(0f, 0f, leftAngle);
            centerPeachMotion.localRotation = Quaternion.Euler(0f, 0f, centerAngle);
            rightPeachMotion.localRotation = Quaternion.Euler(0f, 0f, rightAngle);

            windowResponse = Mathf.Lerp(windowResponse, houseProximity, smoothing);
            float flicker = 0.28f + 0.09f * Mathf.Sin(time * 2.8f) + 0.05f * Mathf.Sin(time * 7.1f + 0.8f);
            window.color = new Color(1f, 1f, 1f, Mathf.Clamp01(flicker + windowResponse * 0.38f + tap * 0.2f));

            Vector2 starPointer = pointer - new Vector2(stars.anchoredPosition.x, -stars.anchoredPosition.y) /
                Mathf.Max(0.001f, scale);
            for (int index = 0; index < starImages.Length; index++)
            {
                float wave = 0.5f + 0.5f * Mathf.Sin(time * (0.96f + index % 7 * 0.17f) + index * 2.39996f);
                float twinkle = wave * wave * wave * wave;
                float proximity = hasPointer ? Proximity(starPointer, StarPositions[index], 145f) : 0f;
                RawImage sparkle = starImages[index];
                sparkle.color = new Color(1f, 1f, 1f, Mathf.Clamp01(twinkle * 0.88f + proximity * 0.32f));
                sparkle.rectTransform.localScale = Vector3.one * (0.7f + twinkle * 0.45f + proximity * 0.16f);
            }
        }

        private static bool ReadPointer(out Vector2 position, out bool pressed)
        {
            if (Touchscreen.current is { } touchscreen && touchscreen.primaryTouch.press.isPressed)
            {
                position = touchscreen.primaryTouch.position.ReadValue();
                pressed = touchscreen.primaryTouch.press.wasPressedThisFrame;
                return true;
            }

            if (Mouse.current is { } mouse)
            {
                position = mouse.position.ReadValue();
                pressed = mouse.leftButton.wasPressedThisFrame;
                return true;
            }

            position = Vector2.zero;
            pressed = false;
            return false;
        }

        private static float Proximity(Vector2 pointer, Vector2 focalPoint, float radius)
        {
            return Mathf.Clamp01(1f - Vector2.Distance(pointer, focalPoint) / radius);
        }

        private static RectTransform AddGroup(string name, RectTransform parent)
        {
            var group = new GameObject(name, typeof(RectTransform));
            group.layer = parent.gameObject.layer;
            var transform = (RectTransform)group.transform;
            transform.SetParent(parent, false);
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.offsetMin = Vector2.zero;
            transform.offsetMax = Vector2.zero;
            return transform;
        }

        private static RawImage AddLayer(string name, RectTransform parent, Texture2D texture,
            Vector2 topLeft, Vector2 size, Vector2 pivot)
        {
            var layer = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            layer.layer = parent.gameObject.layer;
            layer.TryGetComponent<RawImage>(out var image);
            image.texture = texture;
            image.raycastTarget = false;

            Vector2 bottomRight = topLeft + size;
            RectTransform transform = image.rectTransform;
            transform.SetParent(parent, false);
            transform.pivot = pivot;
            transform.anchorMin = new Vector2(topLeft.x / ArtworkSize.x, 1f - bottomRight.y / ArtworkSize.y);
            transform.anchorMax = new Vector2(bottomRight.x / ArtworkSize.x, 1f - topLeft.y / ArtworkSize.y);
            transform.offsetMin = Vector2.zero;
            transform.offsetMax = Vector2.zero;
            return image;
        }
    }
}
