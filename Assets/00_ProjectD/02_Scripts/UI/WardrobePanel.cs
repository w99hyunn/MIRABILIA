using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DoHwa.UI
{
    public sealed class WardrobePanel : MonoBehaviour
    {
        [SerializeField] private CharacterWardrobe wardrobe;
        [SerializeField] private RpgUiController owner;
        [SerializeField] private UnityEngine.UI.RawImage preview;
        [SerializeField] private UnityEngine.UI.Button[] tabs;
        [SerializeField] private Transform content;
        [SerializeField] private WardrobeItemView itemTemplate;
        [SerializeField] private TMP_Text detail;
        [SerializeField] private TMP_Text itemTitle;
        [SerializeField] private UnityEngine.UI.Image itemArt;
        [SerializeField] private TMP_Text status;
        [SerializeField] private TMP_Text directionLabel;
        private readonly List<WardrobeItemView> cards = new();
        [SerializeField] private WardrobeItemView[] initialCards;
        [SerializeField] private DohwaLayeredCharacter previewCharacter;
        [SerializeField] private Camera previewCamera;
        public DohwaLayeredCharacter PreviewCharacter => previewCharacter;
        public Camera PreviewCamera => previewCamera;
        private string[] draft;
        private int slot;
        private int direction;
        private bool walking;
        private static readonly string[] SlotNames = { "머리", "상의", "하의", "신발", "장식" };
        private static readonly string[] Directions = { "정면", "오른쪽 앞", "오른쪽", "오른쪽 뒤", "뒷면", "왼쪽 뒤", "왼쪽", "왼쪽 앞" };

        private void Awake()
        {
            cards.AddRange(initialCards);
            for (int i = 0; i < tabs.Length; i++) { int index = i; tabs[i].onClick.AddListener(() => SelectSlot(index)); }
        }

        private void OnEnable()
        {
            wardrobe.Initialize();
            draft = wardrobe.Snapshot();
            previewCharacter.gameObject.SetActive(true);
            direction = 0;
            directionLabel.text = Directions[0];
            walking = false;
            wardrobe.Render(previewCharacter, draft);
            SelectSlot(0);
        }

        private void OnDisable() => previewCharacter.gameObject.SetActive(false);

        private void LateUpdate()
        {
            previewCharacter.Animate((AvatarFacing)direction, walking ? AvatarMotion.Walk : AvatarMotion.Idle, Time.unscaledDeltaTime, 1);
            previewCamera.Render();
        }

        public void SelectSlot(int value)
        {
            slot = value;
            RefreshCards();
        }

        private void RefreshCards()
        {
            int visibleCount = 0;
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].TryGetComponent<UiVisualState>(out var visual);
                visual.SetActiveState(i == slot);
            }
            var equipped = wardrobe.Snapshot();
            BindCard(visibleCount++, "미착용", null, "", equipped[slot]);
            foreach (var item in wardrobe.Catalog.items)
                if ((int)item.Slot == slot) BindCard(visibleCount++, item.title, item.icon, item.id, equipped[slot]);
            for (int i = visibleCount; i < cards.Count; i++) cards[i].gameObject.SetActive(false);
            var selected = wardrobe.Catalog.Find(draft[slot]);
            itemTitle.text = selected == null ? SlotNames[slot] + " / 미착용" : selected.title;
            itemArt.sprite = selected == null ? null : selected.icon;
            itemArt.enabled = itemArt.sprite;
            detail.text = selected == null ? "이 부위의 코디를 착용하지 않습니다." : selected.description;
            bool changed = false;
            for (int i = 0; i < 5; i++) changed |= draft[i] != equipped[i];
            status.text = changed ? "미리보기 중 / 적용하면 코디가 저장됩니다" : "현재 착용 중인 코디입니다";
        }

        private void BindCard(int index, string label, Sprite icon, string id, string equippedId)
        {
            if (index == cards.Count) cards.Add(Instantiate(itemTemplate, content));
            var card = cards[index];
            card.gameObject.SetActive(true);
            card.Bind(this, id, label, icon, draft[slot] == id, equippedId == id);
        }

        public void Choose(string id)
        {
            draft[slot] = id;
            wardrobe.Render(previewCharacter, draft);
            RefreshCards();
        }
        public void Rotate(int step) { direction = (direction + step + 8) % 8; directionLabel.text = Directions[direction]; }
        public void ToggleWalk() { walking = !walking; directionLabel.text = Directions[direction] + (walking ? " / 걷기" : " / 대기"); }
        public void ResetOutfit() { draft = (string[])wardrobe.Catalog.defaults.Clone(); wardrobe.Render(previewCharacter, draft); RefreshCards(); }
        public void Apply() { wardrobe.Apply(draft); owner.Close(); }
        public void Cancel() => owner.Close();

    }
}

