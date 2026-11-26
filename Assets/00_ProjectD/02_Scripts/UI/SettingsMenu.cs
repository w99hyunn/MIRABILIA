using UnityEngine;
using UnityEngine.UI;

namespace DoHwa.UI
{
    public sealed class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private GameObject preferences;
        [SerializeField] private Image settingsTab, exitTab;
        [SerializeField] private Toggle settingsSelection, coopSelection;
        [SerializeField] private CoopRoomPanel coopPanel;
        [SerializeField] private WorldNavigation worldNavigation;
        private void OnEnable()
        {
            preferences.SetActive(true);
            coopPanel.ShowSettings();
            settingsSelection.isOn=true;
            coopSelection.SetIsOnWithoutNotify(false);
        }
        public void ShowSettings()
        {
            bool changed=!preferences.activeSelf;
            coopPanel.ShowSettings();settingsSelection.isOn=true;
            coopSelection.SetIsOnWithoutNotify(false);
            if(changed && TryGetComponent<UiPanelReveal>(out var reveal))reveal.RefreshContent(0);
        }
        public void ShowCoop()
        {
            bool changed=!coopPanel.gameObject.activeSelf;
            coopPanel.ShowRoom();coopSelection.isOn=true;
            settingsSelection.SetIsOnWithoutNotify(false);
            if(changed && TryGetComponent<UiPanelReveal>(out var reveal))reveal.RefreshContent(1);
        }
        public void ShowExit() => SystemUI.ConfirmQuit();
        public void ShowReturnToMain() => SystemUI.Confirm("메인 메뉴",
            "현재 방을 나가고 메인 메뉴로 돌아가시겠습니까?", "돌아가기", worldNavigation.ReturnToMain);
    }
}
