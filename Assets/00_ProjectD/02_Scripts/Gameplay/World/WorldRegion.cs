using System;
using UnityEngine;

namespace DoHwa.World
{
    [Serializable]
    public sealed class WorldRegion
    {
        public string title;
        public string sceneName;
#if UNITY_EDITOR
        [SerializeField] private UnityEditor.SceneAsset editorScene;
        public UnityEditor.SceneAsset EditorScene => editorScene;
#endif
        public string climate="봄 / 맑음";
        public bool groundCover=true;
        public Vector2 center;
        public Vector2 routeOffset;
        public float elevation;
        public float relief=12;
        public Color ground=Color.gray;
        public Color atmosphere=new(.7f,.75f,.8f);
        public Color sunlight=Color.white;
        public GameObject vegetation;
        public GameObject scenery;
        public GameObject landmark;
        public GameObject pier;
        public GameObject[] roadsideProps=Array.Empty<GameObject>();
        public int vegetationCount=48;
        public bool blossoms;
        public WorldResource timberResource;
        public float lakeRadius;
    }
}
