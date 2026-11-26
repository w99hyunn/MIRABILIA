using System;
using DoHwa.World;
using Steamworks;
using UnityEngine;

namespace DoHwa.UI
{
    public sealed class LoginEntry : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button startButton;
        [SerializeField] private TMPro.TMP_Text startLabel;
        [SerializeField] private MainMenuNavigation navigation;
        [SerializeField] private Animator startAnimator;
        [SerializeField] private CanvasGroup startGroup;
        [SerializeField] private UnityEngine.UI.Image confetti;
        private Material confettiMaterial;
        private void Awake(){confettiMaterial=new Material(confetti.material);confetti.material=confettiMaterial;}
        private void OnDestroy()=>Destroy(confettiMaterial);
        private bool entering;
        public static bool SignedIn { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()=>SignedIn=false;
        public void Configure(UnityEngine.UI.Button button,TMPro.TMP_Text label) { startButton=button; startLabel=label; }
        private string idleLabel;
        private void ShowLoginError(string message)
        {
            confettiMaterial.SetFloat("_Clicked",0);
            entering=false;startGroup.blocksRaycasts=true;startGroup.interactable=true;
            startLabel.text=idleLabel;startAnimator.Play("Normal",0,0);
            SystemUI.ShowNotice("로그인 안내", message);
        }
        public async void Enter()
        {
            if(entering || !startButton.interactable)return;
            entering=true;startGroup.blocksRaycasts=false;startGroup.interactable=false;
            foreach(var parameter in startAnimator.parameters)
                if(parameter.type==AnimatorControllerParameterType.Trigger)startAnimator.ResetTrigger(parameter.nameHash);
            confettiMaterial.SetFloat("_Clicked",1);
            idleLabel=startLabel.text;
            startLabel.text="접속 중";
            startAnimator.CrossFadeInFixedTime("Commit",.08f,0,0);
            string message;
            try
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);
                while(startAnimator.IsInTransition(0) || !startAnimator.GetCurrentAnimatorStateInfo(0).IsName("Commit") || startAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime<1)
                    await Awaitable.NextFrameAsync(destroyCancellationToken);
                if(SteamRuntime.Ready && SteamUser.BLoggedOn())
                {
                    SignedIn=true;
                    navigation.CompleteLogin();
                    return;
                }
                message="Steam에 로그인한 후 다시 시도해 주세요.";
            }
            catch(OperationCanceledException){return;}
            catch(Exception exception)
            {
                Debug.LogException(exception);
                message="로그인하지 못했습니다. 잠시 후 다시 시도해 주세요.";
            }
            ShowLoginError(message);
        }
    }
}






