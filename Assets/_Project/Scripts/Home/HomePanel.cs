using System;
using Dreamy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Home
{
    public sealed class HomePanel : UIPanel, IHomeView
    {
        [Header("Navigation")]
        [SerializeField] private Button shopButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button demoButton;
        [SerializeField] private Button dailyRewardButton;
        [SerializeField] private Button missionsButton;

        public override bool CanBack => false;
        public override bool CanCache => true;

        public event Action<HomeDestination> DestinationRequested;

        private void OnEnable()
        {
            shopButton?.onClick.AddListener(RequestShop);
            settingsButton?.onClick.AddListener(RequestSettings);
            dailyRewardButton?.onClick.AddListener(RequestDailyReward);
            missionsButton?.onClick.AddListener(RequestMissions);
            if (demoButton != null)
            {
                demoButton.onClick.AddListener(RequestDemo);
            }
        }

        protected override void OnDisable()
        {
            shopButton?.onClick.RemoveListener(RequestShop);
            settingsButton?.onClick.RemoveListener(RequestSettings);
            dailyRewardButton?.onClick.RemoveListener(RequestDailyReward);
            missionsButton?.onClick.RemoveListener(RequestMissions);
            demoButton?.onClick.RemoveListener(RequestDemo);
            base.OnDisable();
        }

        public void SetNavigationInteractable(bool interactable)
        {
            if (shopButton != null) shopButton.interactable = interactable;
            if (settingsButton != null) settingsButton.interactable = interactable;
            if (demoButton != null) demoButton.interactable = interactable;
            if (dailyRewardButton != null) dailyRewardButton.interactable = interactable;
            if (missionsButton != null) missionsButton.interactable = interactable;
        }

        private void RequestDemo() => DestinationRequested?.Invoke(HomeDestination.Demo);
        private void RequestDailyReward() => DestinationRequested?.Invoke(HomeDestination.DailyReward);
        private void RequestMissions() => DestinationRequested?.Invoke(HomeDestination.Missions);

        private void RequestShop() => DestinationRequested?.Invoke(HomeDestination.Shop);
        private void RequestSettings() => DestinationRequested?.Invoke(HomeDestination.Settings);
    }
}
