using UnityEngine;

namespace DoHwa.World
{
    public sealed class DokkaebiGate : MonoBehaviour
    {
        [SerializeField] private DokkaebiPactDefinition pact;
        [SerializeField] private string gateId;
        [SerializeField] private bool returnGate;
        [SerializeField] private string destinationScene;
        [SerializeField] private Vector3 arrivalPosition;
        [SerializeField] private ParticleSystem flames;
        public string Id => gateId;
        public bool Open { get; private set; }
        public string Hint => returnGate ? "[E] 도화림으로 돌아가기" : Open
            ? "[E] 달그늘 길로 들어가기" : "[E] " + pact.offeringHint;

        public void Render(WorldState state)
        {
            Open = state.pacts.Contains(pact.id);
            var emission = flames.emission;
            emission.rateOverTime = Open ? 28 : 5;
        }
        public bool Interact(WorldState state, GameCatalog catalog, SurvivorState player, out string reason)
        {
            if (!returnGate && !state.pacts.Contains(pact.id))
            {
                if (!WorldRules.Transact(player, catalog, pact.offering,
                    System.Array.Empty<ItemAmount>(), out reason)) return false;
                state.pacts.Add(pact.id);
                reason = "약속이 맺어졌습니다. 달그늘 길이 열렸습니다";
                return true;
            }
            if (!state.pacts.Contains(pact.id)) { reason = "아직 열리지 않은 길입니다"; return false; }
            player.mapScene = destinationScene;
            player.position = arrivalPosition;
            player.travelRevision++;
            reason = returnGate ? "도화림으로 돌아왔습니다" : "달그늘 길 — 파수꾼의 봉인을 풀어 주세요";
            return true;
        }
    }
}


