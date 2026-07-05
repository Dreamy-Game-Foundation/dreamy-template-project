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

            // 1. Dọn dẹp trước các placeholder/dummy items để tránh đăng ký sai tween
            ClearSpawnedItems();

            // 2. Tải và tạo mới các Offer Items
            await LoadOffers();

            // 3. Khởi tạo lại TweenPlayer để nhận dạng và lưu danh sách ITween của các item vừa sinh ra
            if (tweenPlayer != null)
            {
                await tweenPlayer.Init();
            }

            // 4. Gắn và kích hoạt TweenDelayControl có sẵn của hệ thống để phân bổ delay tự động
            var delayControl = this.GetOrAddComponent<TweenDelayControl>();
            if (delayControl != null)
            {
                delayControl.ApplyDelays();
            }
        }

        private void OnEnable()
        {
            closeButton.onClick.AddListener(OnClose);
            OnPreHide += InitTweenPlayer;
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(OnClose);
            OnPreHide -= InitTweenPlayer;
        }

        void InitTweenPlayer() => tweenPlayer?.Init();

        private async UniTask LoadOffers()
        {
            var dataConfigService = ServiceLocator.Get<IDataConfigService>();
            if (dataConfigService == null) return;

            var offerTable = dataConfigService.GetTable<OfferConfigTable>();
            if (offerTable == null) return;

            // Phân loại offer theo Gem và Gold tương tự mẫu tham khảo
            var gemOffers = offerTable.GetOffersByType(EResourceOffer.Gem);
            var goldOffers = offerTable.GetOffersByType(EResourceOffer.Gold);

            // Tải bất đồng bộ prefab bằng AssetLoader
            uiGemOfferPrefab = await AssetLoader.LoadAsync<GameObject>(Address.UIShopResourceOffer_Gem);
            uiGoldOfferPrefab = await AssetLoader.LoadAsync<GameObject>(Address.UIShopResourceOffer_Gold);

            // Sinh các UI Gem Offer bằng Instantiate
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

            // Sinh các UI Gold Offer bằng Instantiate
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
        }

        private void ClearSpawnedItems()
        {
            if (gemOfferHolder != null) gemOfferHolder.DestroyChildren();
            if (goldOfferHolder != null) goldOfferHolder.DestroyChildren();
            foreach (var item in spawnedItems)
            {
                if (item != null) Destroy(item);
            }

            spawnedItems.Clear();
        }

        private void OnClose() => Hide().Forget();

        protected override void OnDestroy()
        {
            Destroyed?.Invoke();
            base.OnDestroy();
        }
    }
}