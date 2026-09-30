using System;
using System.Collections.Generic;
using Dreamy.DailyReward;
using Dreamy.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Feature.DailyReward.Integration
{
    public sealed class DailyRewardPanel : UIPanel, IDailyRewardView
    {
        [SerializeField] private Button claimButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Transform rewardContainer;
        [SerializeField] private DailyRewardRewardItem rewardItemPrefab;
        [SerializeField] private GameObject coinHolderPrefab;
        [SerializeField] private GameObject gemHolderPrefab;
        private readonly List<DailyRewardRewardItem> items = new();
        private GameObject coinHolder;
        private GameObject gemHolder;

        public override bool CanBack => true;
        public event Action ClaimRequested;
        public event Action CloseRequested;
        public event Action Destroyed;

        private void OnEnable()
        {
            if (coinHolder == null) coinHolder = CreateHolder(coinHolderPrefab, -125f);
            if (gemHolder == null) gemHolder = CreateHolder(gemHolderPrefab, 125f);
            claimButton.onClick.AddListener(RequestClaim);
            closeButton.onClick.AddListener(RequestClose);
        }

        protected override void OnDisable()
        {
            claimButton.onClick.RemoveListener(RequestClaim);
            closeButton.onClick.RemoveListener(RequestClose);
            base.OnDisable();
        }

        public void Render(DailyRewardViewState state)
        {
            EnsureItems(state.Rewards.Count);
            for (int index = 0; index < state.Rewards.Count; index++) items[index].Render(state.Rewards[index]);
            statusText.text = state.CanClaim ? "Reward available" : $"Next reward: {state.NextClaimUtc:yyyy-MM-dd}";
        }

        public void SetClaimInteractable(bool interactable) => claimButton.interactable = interactable;

        public void ShowClaimResult(DailyRewardClaimResult result) => statusText.text = result.Status.ToString();

        public void Close() => Hide();

        protected override void OnDestroy()
        {
            foreach (DailyRewardRewardItem item in items) if (item != null) Destroy(item.gameObject);
            Destroyed?.Invoke();
            base.OnDestroy();
        }

        private void EnsureItems(int count)
        {
            if (rewardItemPrefab == null || rewardContainer == null) throw new InvalidOperationException("Assign a reward item prefab and container.");
            while (items.Count < count) items.Add(Instantiate(rewardItemPrefab, rewardContainer));
        }

        private void RequestClaim() => ClaimRequested?.Invoke();
        private void RequestClose() => CloseRequested?.Invoke();

        private GameObject CreateHolder(GameObject prefab, float x)
        {
            if (prefab == null) return null;
            GameObject holder = Instantiate(prefab, transform);
            RectTransform rect = holder.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(x, -70f);
            return holder;
        }
    }
}
