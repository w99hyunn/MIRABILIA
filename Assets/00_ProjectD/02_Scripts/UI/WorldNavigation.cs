using DoHwa.World;
using UnityEngine;

namespace DoHwa.UI
{
    public sealed class WorldNavigation : MonoBehaviour
    {
        [SerializeField] private WorldRuntime world;
        [SerializeField] private CoopSession session;

        public void ReturnToMain()
        {
            if (session.Busy) return;
            if (world.IsAuthority) world.Save();
            session.Leave();
        }
    }
}
