using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Assets;
using Dreamy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    public sealed class UIShopPanel : UIPanel
    {
        [Header("Controls")] [SerializeField] private Button closeButton;

        [Header("Holders")] [SerializeField] private Transform gemOfferHolder;
        [SerializeField] private Transform goldOfferHolder;

        private GameObject uiGemOfferPrefab;
        private GameObject uiGoldOfferPrefab;

        public override bool CanBack => true;

        public event Action Destroyed;

        private readonly List<GameObject> spawnedItems = new();

        public override async UniTask Init()
        {
            await base.Init();

            await LoadOffers();

            if (tweenPlayer != null)
            {
                await tweenPlayer.Init();
            }

            var delayControl = this.GetOrAddComponent<TweenDelayControl>();
            if (delayControl != null)
            {
                delayControl.ApplyDelays();
            }
        }


        private void Awake()
        {
            closeButton.onClick.AddListener(OnClose);
        }

        private async UniTask LoadOffers()
        {
            ClearSpawnedItems();

            var dataConfigService = ServiceLocator.Get<IDataConfigService>();
            if (dataConfigService == null) return;

            var offerTable = dataConfigService.GetTable<OfferConfigTable>();
            if (offerTable == null) return;

            var gemOffers = offerTable.GetOffersByType(EResourceOffer.Gem);
            var goldOffers = offerTable.GetOffersByType(EResourceOffer.Gold);

            uiGemOfferPrefab = await AssetLoader.LoadAsync<GameObject>(Address.UIShopResourceOffer_Gem);
            uiGoldOfferPrefab = await AssetLoader.LoadAsync<GameObject>(Address.UIShopResourceOffer_Gold);

            foreach (var offer in gemOffers)
            {
                if (uiGemOfferPrefab != null && gemOfferHolder != null)
                {
                    var itemGo = Instantiate(uiGemOfferPrefab, gemOfferHolder, false);
                    var uiGemOffer = itemGo.GetComponent<UIResourceOfferItem>();
                    if (uiGemOffer != null)
                    {
                        uiGemOffer.Setup(offer);
                    }

                    spawnedItems.Add(itemGo);
                }
                else
                {
                    Debug.Log($"[UIShopPanel] Gem Offer (No Prefab/Holder): {offer.Name} | Price: {offer.Price}");
                }
            }

            foreach (var offer in goldOffers)
            {
                if (uiGoldOfferPrefab != null && goldOfferHolder != null)
                {
                    var itemGo = Instantiate(uiGoldOfferPrefab, goldOfferHolder, false);
                    var uiGoldOffer = itemGo.GetComponent<UIResourceOfferItem>();
                    if (uiGoldOffer != null)
                    {
                        uiGoldOffer.Setup(offer);
                    }

                    spawnedItems.Add(itemGo);
                }
                else
                {
                    Debug.Log($"[UIShopPanel] Gold Offer (No Prefab/Holder): {offer.Name} | Price: {offer.Price}");
                }
            }

            Canvas.ForceUpdateCanvases();
        }


        private void ClearSpawnedItems()
        {
            gemOfferHolder.DestroyChildrenImmediate();
            goldOfferHolder.DestroyChildrenImmediate();
            foreach (var item in spawnedItems)
            {
                if (item != null) DestroyImmediate(item);
            }

            spawnedItems.Clear();
        }

        private void OnClose() => Hide().Forget();

        protected override void OnDestroy()
        {
            Destroyed?.Invoke();
            closeButton.onClick.RemoveListener(OnClose);
            base.OnDestroy();
        }
    }
}