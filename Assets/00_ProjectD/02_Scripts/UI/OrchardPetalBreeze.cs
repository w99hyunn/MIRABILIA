using UnityEngine;
using UnityEngine.UI;

namespace DoHwa.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class OrchardPetalBreeze : MaskableGraphic
    {
        [SerializeField] private Texture2D drawings;
        private const float LOOP_SECONDS = 12.8f;

        private readonly struct Petal
        {
            public readonly Vector2 tip, landing;
            public readonly float release, flight, width;
            public readonly bool water;

            public Petal(float x, float y, float endX, float endY, float release, float flight, float width, bool water = false)
            {
                tip = new Vector2(x, y);
                landing = new Vector2(endX, endY);
                this.release = release;
                this.flight = flight;
                this.width = width;
                this.water = water;
            }
        }

        // Individual blossom edges and visible banks, rocks or water in painting pixels (Y down).
        private static readonly Petal[] petals =
        {
            new(1144, 525,  861, 801,  0.16f, 5.7f, 21),
            new(1350, 299,  776, 749,  0.84f, 7.9f, 23),
            new(1334, 507, 1130, 813,  1.51f, 5.0f, 28),
            new(1455, 171,  903, 785,  2.42f, 8.8f, 22),
            new(1223, 321,  747, 834,  3.05f, 7.1f, 23),
            new(1524, 478, 1312, 752,  3.38f, 5.8f, 26),
            new(1138, 517, 1000, 817,  4.31f, 5.0f, 24, true),
            new(1384, 225,  810, 770,  5.24f, 8.3f, 20),
            new(1597, 518, 1353, 733,  5.62f, 4.9f, 23),
            new(1285, 317,  915, 742,  6.80f, 6.9f, 21),
            new(1108, 485,  873, 837,  7.78f, 5.6f, 25, true),
            new(1508, 181,  965, 743,  8.57f, 8.6f, 24),
            new(1447, 451, 1169, 765,  9.14f, 6.1f, 24),
            new(1157, 405,  763, 786,  9.72f, 6.8f, 20),
            new(1336, 306,  894, 773, 10.97f, 7.6f, 22),
            new(1199, 520, 1060, 860, 11.53f, 5.5f, 26, true),
            new(1573, 134, 1073, 749, 12.17f, 8.7f, 22)
        };

        public override Texture mainTexture => drawings;

        private void Update()
        {
            if (Application.isPlaying) SetVerticesDirty();
        }

        // Integral of one common, always leftward wind. Petals already in the air
        // pick up the same gust as the ones that have just left the tree.
        private static float WindTravel(float time)
        {
            float phase = time * (2f * Mathf.PI / LOOP_SECONDS);
            return time - 0.96f * Mathf.Sin(phase) - 0.34f * Mathf.Sin(2f * phase + 0.6f);
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (!drawings) return;
            Rect rect = rectTransform.rect;
            float scale = rect.width / 1672f;
            float time = Mathf.Repeat(Time.time, LOOP_SECONDS);
            foreach (var p in petals)
            {
                // One second attached before release; time after impact belongs to the landing.
                float elapsed = Mathf.Repeat(time - p.release + 1f, LOOP_SECONDS) - 1f;
                float age = Mathf.Clamp01(elapsed / p.flight);
                float landed = Mathf.Max(0, elapsed - p.flight);
                if (landed > 1.9f) continue;

                float elapsedAir = Mathf.Clamp(elapsed, 0, p.flight);
                float easedTime = elapsedAir * Mathf.SmoothStep(0, 1, elapsedAir / 0.55f);
                float travel = (WindTravel(p.release + easedTime) - WindTravel(p.release))
                             / (WindTravel(p.release + p.flight) - WindTravel(p.release));
                float fall = age * age * (2f - age);
                float bend = Mathf.Sin(Mathf.PI * age);
                float turn = 0.013f * p.tip.x + elapsedAir * (1.8f + p.width * 0.026f);
                var pos = new Vector2(Mathf.Lerp(p.tip.x, p.landing.x, travel),
                                      Mathf.Lerp(p.tip.y, p.landing.y, fall));
                pos += new Vector2(9f * Mathf.Sin(turn), 7f * Mathf.Cos(turn) - 15f) * bend;
                if (p.water) pos += new Vector2(-11f, 1.2f) * landed;

                float rest = Mathf.SmoothStep(0, 1, (age - 0.94f) / 0.06f);
                float tilt = Mathf.Lerp(0.24f + 0.65f * Mathf.Sin(turn), 0.06f, rest);
                int cel = Mathf.FloorToInt(elapsedAir * 4f + p.width) % 8;
                Color tint = color;
                tint.a *= 0.86f * Mathf.SmoothStep(0, 1, (elapsed + 1f) / 0.7f)
                        * (1f - Mathf.SmoothStep(0, 1, (landed - 0.55f) / 1.35f));
                Vector2 center = new(rect.xMin + pos.x * scale, rect.yMax - pos.y * rect.height / 941f);
                DrawCel(mesh, center, new Vector2(p.width, p.width * Mathf.Lerp(0.5f, 0.21f, rest)) * scale,
                        tilt, cel, true, tint);

                if (p.water && landed > 0f && landed < 1.1f)
                {
                    Vector2 contact = new(rect.xMin + p.landing.x * scale, rect.yMax - p.landing.y * rect.height / 941f);
                    DrawCel(mesh, contact, new Vector2(64f, 32f) * scale, 0f,
                            Mathf.FloorToInt(landed * (8f / 1.1f)), false, color);
                }
            }
        }

        private static void DrawCel(VertexHelper mesh, Vector2 center, Vector2 size, float tilt, int cel, bool leaf, Color tint)
        {
            var axis = new Vector2(Mathf.Cos(tilt), Mathf.Sin(tilt));
            var right = axis * (size.x * 0.5f);
            var up = new Vector2(-axis.y, axis.x) * (size.y * 0.5f);
            float u0 = (cel + 0.008f) / 8f, u1 = (cel + 0.992f) / 8f;
            float v = leaf ? 0.5f : 0f;
            int first = mesh.currentVertCount;
            mesh.AddVert(center - right - up, tint, new Vector2(u0, v + 0.008f));
            mesh.AddVert(center - right + up, tint, new Vector2(u0, v + 0.492f));
            mesh.AddVert(center + right + up, tint, new Vector2(u1, v + 0.492f));
            mesh.AddVert(center + right - up, tint, new Vector2(u1, v + 0.008f));
            mesh.AddTriangle(first, first + 1, first + 2);
            mesh.AddTriangle(first, first + 2, first + 3);
        }
    }
}
