using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Assets;
using Dreamy.UI;
using Dreamy.Template.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Demo {
    public sealed class UIShopPanel : UIPanel {
        [Header("Controls")] [SerializeField] private Button closeButton;

        [Header("Holders")] [SerializeField] private Transform gemOfferHolder;
        [SerializeField] private Transform goldOfferHolder;

        private GameObject uiGemOfferPrefab;
        private GameObject uiGoldOfferPrefab;

        public override bool CanBack => true;

        public event Action Destroyed;

        private readonly List<GameObject> spawnedItems = new();

        private void Awake() {
            OnPreShow += SpawnOffers;
            OnPostHide += ClearSpawnedItems;
        }

        public override async UniTask PostInit() {
            // Tải bất đồng bộ prefab bằng AssetLoader một lần và lưu trữ
            uiGemOfferPrefab = await AssetLoader.LoadAsync<GameObject>(Address.UIShopResourceOffer_Gem);
            uiGoldOfferPrefab = await AssetLoader.LoadAsync<GameObject>(Address.UIShopResourceOffer_Gold);
            await base.PostInit();
        }

        private void OnEnable() {
            closeButton.onClick.AddListener(OnClose);
        }

        private void OnDisable() {
            closeButton.onClick.RemoveListener(OnClose);
        }

        private void SpawnOffers() {
            ClearSpawnedItems();
            gemOfferHolder.DestroyChildren();
            goldOfferHolder.DestroyChildren();

            var dataConfigService = ServiceLocator.Get<IDataConfigService>();
            if (dataConfigService == null) return;

            var offerTable = dataConfigService.GetTable<OfferConfigTable>();
            if (offerTable == null) return;

            var gemOffers = offerTable.GetOffersByType(EResourceOffer.Gem);
            var goldOffers = offerTable.GetOffersByType(EResourceOffer.Gold);

            // Sinh các UI Gem Offer từ Pools
            foreach (var offer in gemOffers) {
                if (uiGemOfferPrefab != null && gemOfferHolder != null) {
                    var uiGemOffer = Pools.Spawn<UIResourceOfferItem>(uiGemOfferPrefab, gemOfferHolder);
                    uiGemOffer.Setup(offer);
                    spawnedItems.Add(uiGemOffer.gameObject);
                }
                else {
                    Debug.Log($"[UIShopPanel] Gem Offer (No Prefab/Holder): {offer.Name} | Price: {offer.Price}");
                }
            }

            gemOfferHolder.GetOrAddComponent<TweenDelayControl>()?.ApplyDelays();

            // Sinh các UI Gold Offer từ Pools
            foreach (var offer in goldOffers) {
                if (uiGoldOfferPrefab != null && goldOfferHolder != null) {
                    var uiGoldOffer = Pools.Spawn<UIResourceOfferItem>(uiGoldOfferPrefab, goldOfferHolder);
                    uiGoldOffer.Setup(offer);
                    spawnedItems.Add(uiGoldOffer.gameObject);
                }
                else {
                    Debug.Log($"[UIShopPanel] Gold Offer (No Prefab/Holder): {offer.Name} | Price: {offer.Price}");
                }
            }

            goldOfferHolder.GetOrAddComponent<TweenDelayControl>()?.ApplyDelays();

            // Khởi tạo lại tweenPlayer để tìm và gán các tween từ các item vừa sinh
            if (tweenPlayer != null) {
                tweenPlayer.Init().Forget();
            }
        }

        private void ClearSpawnedItems() {
            foreach (var item in spawnedItems) {
                if (item != null) Pools.Despawn(item);
            }

            spawnedItems.Clear();
        }

        private void OnClose() => Hide().Forget();

        protected override void OnDestroy() {
            OnPreShow -= SpawnOffers;
            OnPostHide -= ClearSpawnedItems;
            Destroyed?.Invoke();
            base.OnDestroy();
        }
    }
}