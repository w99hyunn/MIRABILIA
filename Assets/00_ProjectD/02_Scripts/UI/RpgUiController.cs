using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DoHwa.UI
{
    public sealed class RpgUiController : SceneScreen
    {
        [SerializeField] private GameObject inventory;
        [SerializeField] private GameObject character;
        [SerializeField] private GameObject map;
        [SerializeField] private GameObject journal;
        [SerializeField] private GameObject settings;
        [SerializeField] private GameObject hud;
        [SerializeField] private TMP_Text feedback;
        [SerializeField] private CraftResultDialog craftResult;
        [SerializeField] private UnityEngine.UI.Slider volumeSlider;
        [SerializeField] private HeroMotor motor;
        [SerializeField] private HeroCamera view;
        [SerializeField] private GameObject wardrobe;
        [SerializeField] private InventoryController inventoryController;
        [SerializeField] private InventorySortMenu sortMenu;
        [SerializeField] private UiBackdropBlur backdropBlur;
        [SerializeField] private Canvas menuCanvas;
        [SerializeField] private DoHwa.World.WorldMapSession maps;
        [SerializeField] private QuickSlotBar quickBar;
        [SerializeField] private WorldDebugPanel debugPanel;
        private bool wasViewEnabled, modal, capturingBackdrop, preparingMenus, menusReady;
        private int transition;
        private float feedbackUntil;
        public bool IsModalOpen => modal;
        private async void Start()
        {
            try { await PrepareMenusAsync(); }
            catch(OperationCanceledException) { }
        }

        internal override Awaitable ReleaseContentAsync() => maps.UnloadMapsAsync();

        internal override Awaitable PrepareForRevealAsync() => PrepareMenusAsync();

        public async Awaitable PrepareMenusAsync()
        {
            while(preparingMenus)await Awaitable.NextFrameAsync(destroyCancellationToken);
            if(menusReady)return;
            preparingMenus=true;
            var canvas = menuCanvas;
            bool wasVisible=canvas.enabled;
            canvas.enabled=false;
            var panels=new[]{inventory,character,map,journal,settings};
            try
            {
                await backdropBlur.PrepareAsync();
                foreach(var panel in panels)
                {
                    bool wasActive=panel.activeSelf;
                    panel.SetActive(true);
                    try
                    {
                        Canvas.ForceUpdateCanvases();
                        await Awaitable.NextFrameAsync(destroyCancellationToken);
                    }
                    finally { if(panel)panel.SetActive(wasActive); }
                }
                menusReady=true;
            }
            finally
            {
                preparingMenus=false;
                if(canvas)canvas.enabled=wasVisible;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (modal) RestoreMovement();
        }
        private void Update()
        {
            if (SystemUI.PopupCapturesInput) return;
            if (preparingMenus) return;
            if (feedback.transform.parent.gameObject.activeSelf && Time.unscaledTime > feedbackUntil)
                feedback.transform.parent.gameObject.SetActive(false);
            var keyboard = Keyboard.current;
            if (keyboard == null || debugPanel.HandleInput(keyboard)) return;
            if(craftResult.IsOpen)
            {
                if(keyboard.escapeKey.wasPressedThisFrame) craftResult.Close();
                return;
            }
            if(keyboard.vKey.wasPressedThisFrame) { if(wardrobe.activeSelf) Close(); else OpenWardrobe(); }
            if(keyboard.bKey.wasPressedThisFrame) { if(inventory.activeSelf && !inventoryController.IsCrafting) Close(); else OpenInventory(); }
            if(keyboard.kKey.wasPressedThisFrame) { if(character.activeSelf) Close(); else OpenEquipment(); }
            if(keyboard.cKey.wasPressedThisFrame) { if(inventory.activeSelf && inventoryController.IsCrafting) Close(); else OpenCrafting(); }
            if(keyboard.mKey.wasPressedThisFrame) { if(map.activeSelf) Close(); else OpenMap(); }
            if(keyboard.jKey.wasPressedThisFrame) { if(journal.activeSelf) Close(); else OpenJournal(); }
            if (!modal && keyboard.qKey.wasPressedThisFrame) quickBar.Toggle();
            if(keyboard.escapeKey.wasPressedThisFrame) { if(quickBar.IsOpen) quickBar.Toggle(); else if(modal) Close(); else OpenSettings(); }
            if (!modal)
                for (int i = 0; i < quickBar.Count; i++)
                    if (keyboard[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
                        inventoryController.UseQuick(i);
        }

        private async void Show(GameObject panel)
        {
            quickBar.Close();
            if(!menusReady)await PrepareMenusAsync();
            bool alreadyOpen = panel.activeSelf;
            if(alreadyOpen && modal && panel.TryGetComponent<UiPanelReveal>(out var panelMotion) && !panelMotion.IsLeaving)return;
            int current=++transition;
            if(craftResult.IsOpen) craftResult.Close();
            if(!modal) {
                wasViewEnabled=view.enabled; motor.SetInputBlocked(HeroMotor.InputBlock.Menu, true); view.enabled=false; modal=true;
                hud.SetActive(false);
                capturingBackdrop=true;
                await backdropBlur.CaptureAsync();
                capturingBackdrop=false;
                if(!this) return;
                if(current!=transition) { if(!modal) backdropBlur.Hide(); return; }
            }
            while(capturingBackdrop) await Awaitable.NextFrameAsync(destroyCancellationToken);
            if(!this || current!=transition) return;
            GameObject outgoing = wardrobe.activeSelf ? wardrobe : character.activeSelf ? character : inventory.activeSelf ? inventory : map.activeSelf ? map : journal.activeSelf ? journal : settings.activeSelf ? settings : null;
            bool wardrobeSwitch = outgoing != panel &&
                ((outgoing == character && panel == wardrobe) || (outgoing == wardrobe && panel == character));
            if (wardrobeSwitch)
            {
                panel.SetActive(true);
                panel.TryGetComponent<UiPanelReveal>(out var incomingMotion);
                outgoing.TryGetComponent<UiPanelReveal>(out var outgoingMotion);
                incomingMotion.ReplayState("SwitchIn");
                await outgoingMotion.HideAsync("SwitchOut");
                if(!this || current != transition) return;
            }
            else if(outgoing && outgoing != panel && outgoing.TryGetComponent<UiPanelReveal>(out var exitMotion))
            {
                await exitMotion.HideAsync();
                if(!this || current != transition) return;
            }
            inventory.SetActive(panel==inventory); map.SetActive(panel==map); journal.SetActive(panel==journal); settings.SetActive(panel==settings);
            character.SetActive(panel==character);
            wardrobe.SetActive(panel==wardrobe);
            hud.SetActive(false);
            if(alreadyOpen && !wardrobeSwitch && panel.TryGetComponent<UiPanelReveal>(out var reveal))reveal.Replay();
        }

        private void RestoreMovement()
        {
            // Scene teardown may destroy the character before the UI is disabled.
            if(motor) motor.SetInputBlocked(HeroMotor.InputBlock.Menu, false);
            if(view) view.enabled=wasViewEnabled;
            modal=false;
        }
        public async void Close()
        {
            if(craftResult.IsOpen) { craftResult.Close(); return; }
            if(inventory.activeSelf && sortMenu.IsOpen) { sortMenu.Hide(); return; }
            int current = ++transition;
            GameObject panel = wardrobe.activeSelf ? wardrobe : character.activeSelf ? character : inventory.activeSelf ? inventory : map.activeSelf ? map : journal.activeSelf ? journal : settings;
            if(panel.activeSelf && panel.TryGetComponent<UiPanelReveal>(out var reveal)) await reveal.HideAsync();
            if(!this || current != transition) return;
            inventory.SetActive(false); map.SetActive(false); journal.SetActive(false); settings.SetActive(false);
            character.SetActive(false);
            wardrobe.SetActive(false);
            hud.SetActive(true);
            backdropBlur.Hide();
            if(modal) RestoreMovement();
        }

        public void OpenInventory()
        {
            inventoryController.PrepareInventory();
            Show(inventory);
        }

        public void OpenEquipment()
        {
            inventoryController.PrepareEquipment();
            Show(character);
        }

        public void OpenCrafting()
        {
            inventoryController.PrepareCrafting();
            Show(inventory);
        }
        public void OpenWardrobe() => Show(wardrobe);
        public void OpenMap() => Show(map);
        public void OpenJournal() => Show(journal);
        public void OpenSettings()
        {
            volumeSlider.SetValueWithoutNotify(AudioListener.volume);
            Show(settings);
        }
        public void SetVolume(float value) => AudioListener.volume=value;
        public void Notify(string message)
        {
            feedback.text=message; feedback.transform.parent.gameObject.SetActive(true); feedbackUntil=Time.unscaledTime+2.8f;
        }

    }
}
