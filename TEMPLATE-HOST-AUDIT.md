# Rà soát Dreamy Template host và kế hoạch chuẩn hóa

Ngày rà soát: 2026-10-04. Phạm vi: host trong `Assets/_Project`, cấu hình package,
scene/prefab, Addressables và tài nguyên mẫu. Báo cáo dựa trên source hiện có,
không phải chứng nhận release hay kết quả đo trên thiết bị.

## 1. Kết luận

Host đã dùng đúng hướng các package nền tảng, nhưng trước lần dọn này chưa đủ
chặt để làm baseline cho nhiều game. Điểm chính là hai Shop cùng tồn tại, demo
save có khái niệm Coins riêng, nhiều sample controller không được dùng, startup
chưa có ownership teardown đầy đủ, và dependency version có drift.

Chọn một đường chính:

- Core cung cấp service registry; GameInstaller là composition root của host.
- Datasave lưu trạng thái; Economy wallet là nguồn sự thật cho tiền/tài nguyên.
- DataConfig chứa thiết kế; Shop dùng ShopCatalogConfig của feature package.
- ShopModel/ShopPresenter thuộc package; ShopPanel/ShopOfferItem là UI integration
  của host đã import và có tùy biến stagger.
- Economy ResourceUIHolder là component hiển thị số dư dùng chung. CoinHolder,
  GemHolder là prefab variant; EconomyHolder là prefab bố cục của game.
- Settings dùng SettingsPanelLauncher/RateUsPanelLauncher theo vòng đời panel.
- Giữ năng lực atlas loading, nhưng demo asset loading phải tách khỏi Shop nghiệp vụ.

## 2. Bằng chứng môi trường và giới hạn compatibility

Unity: `6000.4.12f1` trong `ProjectSettings/ProjectVersion.txt`.
URP 17.4.0 được resolve; `GraphicsSettings.asset` có pipeline asset URP.
Addressables 2.9.1; Input System 1.19.0; UniTask, DOTween, Newtonsoft và LeanPool
đang có trong graph/source. Build settings bật BootstrapScene và MainScene;
scene T bị tắt.

Đã đọc `Packages/manifest.json`, `Packages/packages-lock.json` và source package
cache trước khi sửa API. Không tìm thấy `toolkit.json`, `rules/index.json` hoặc
`compatibility/dreamy-packages.json` trong workspace đã kiểm tra. Vì vậy các
bảng dưới là thông tin quan sát tại checkout, không khẳng định maturity/status
hay preset được toolkit chứng nhận. Không tự tạo registry thay thế canonical.

| Package | version trong package.json cache | hash resolve, rút gọn | host sử dụng |
| --- | --- | --- | --- |
| core | 1.1.1 | cba1ef9e7d60 | ServiceLocator, singleton |
| assets | 0.1.1 | 72d8386068d7 | PanelManager, prefab/atlas loading |
| audio | 0.1.0 | 8eb21c939ce3 | DreamyAudio.Service, Settings, music startup |
| dataconfig | 0.2.0 | 6d199250c544 | config local, shop catalog |
| datasave | 0.2.0 | 9fa6bd218120 | demo save và Economy wallet |
| editor-tools | 0.2.1 | e00ca5cfdf88 | tooling Editor, chưa chạy harness |
| feature | 0.1.0 | 885e27604f9b | dependency của feature |
| feature.economy | 0.2.0 | 64b27adfd6f6 | wallet và ResourceUIHolder |
| feature.settings | 0.2.0 | 9e0e3f0e2dd0 | Settings/RateUs presenter |
| feature.shop | 0.1.1 | f55feb6180e6 | catalog, mua hàng, presenter |
| ui | 0.2.0 | aed41b9229c1 | panel, navigation, tween |

Các tag `v1.0.0` của Assets/DataConfig/Datasave/Editor Tools không trùng version
trong package.json cache. Đây là drift tên tag/version, cần team thống nhất quy
ước release, không tự nâng package trong lần dọn host này. Nhiều dependency yêu
cầu Core 1.1.2 trong khi root đang resolve Core 1.1.1. Compile hiện tại không đủ
để kết luận mọi nhánh của package tương thích. Audio/UI dùng `#main`; feature
packages không pin ref. Lock giữ hash hiện tại, nhưng update package có thể đổi
hành vi. Phase compatibility phải pin bộ phiên bản đã kiểm thử.

## 3. GameInstaller sau chỉnh sửa

Đọc `Assets/_Project/Scripts/Bootstrap/GameInstaller.cs` từ trên xuống theo nhóm:

```text
Awake -> DontDestroyOnLoad -> Initializing
  1. InstallPersistence
     -> IDatasaveService
     InstallInfrastructure
     -> IPoolService, IAudioService
  2. InstallEconomy
     -> cùng một DatasaveResourceWallet
     -> IResourceWallet + IResourceBalanceProvider
  3. InstallConfigurationAsync
     -> TemplateConfig, TestConfigTable, ShopCatalogConfig
     -> await InitializeAsync
     -> IDataConfigService
  4. InstallFeatures
     -> Settings, phụ thuộc Audio
     -> gateway IAP giả chỉ trong UNITY_EDITOR
     -> Shop, nhận catalog + wallet + gateway cụ thể
  -> Ready
```

Lỗi init được ghi vào InitializationException và State=Failed; lỗi cũ được reset
khi bắt đầu init. Không đưa log từng dòng TestConfig vào startup. Dùng đúng
TestConfigTable thay vì đăng ký generic DataConfigTable<TestConfig> rồi để lớp
table chuyên biệt không được sử dụng.

Bỏ RemoteDataConfigProvider luôn trả null và CompositeConfigSource chỉ bọc một
fallback thực tế. Khi có backend thật, thêm provider tại composition root cùng
quy tắc timeout/cancellation/fallback, rồi kiểm thử remote và offline.

Trong Editor, Shop vẫn có simulation để demo. Player tìm gateway đã đăng ký;
không có adapter thật thì package trả IapGatewayUnavailable, không tự cấp reward
nhờ gateway giả. Settings giữ default không có platform gateway: nút platform
không khả dụng/ẩn theo state. Không giả vờ đã mở store hay restore thành công.

Còn tồn tại: installer chưa chống duplicate instance, teardown chỉ unregister
Pool, chưa rollback các service đã cài khi init thất bại. Static state/locator
cần được kiểm thử khi tắt Domain Reload và khi chạy Play Mode nhiều lần. Đây là
phase lifecycle riêng, tránh đổi rộng ownership trong một lần trình bày installer.
DreamyAudio.Service do package sở hữu; host đăng ký tham chiếu, không mặc định
coi nó là object host có quyền Dispose.

## 4. ResourceHolder: dùng và giữ cái nào?

Giữ ResourceUIHolder của `com.dreamy.feature.economy`. Source tại
`Runtime.UI/ResourceUIHolder.cs` đã có ResourceId, Bind/Unbind, catalog, format và
BalanceChanged. Host không cần thêm một ResourceHolder khác để đọc số dư.

Giữ:

| Asset/type | Vai trò | Quyết định |
| --- | --- | --- |
| package ResourceUIHolder | hiển thị tài nguyên chung | giữ package ownership |
| imported CoinHolder/GemHolder | variant với ID/icon/UI override | giữ GUID và prefab inheritance |
| Prefabs/UI/EconomyHolder | ghép hai holder thành cụm UI | giữ, đây là composition |
| legacy UIResourceValue | chỉ SetValue từ enum/int riêng, không bind wallet | đã xóa cùng Shop cũ |

Wallet đăng ký IResourceWallet và IResourceBalanceProvider từ cùng instance.
Mọi thay đổi số dư đi qua TryGrant/TryExchange; holder chỉ hiển thị. Chọn
`currency.coin`, `currency.gem`, `booster.bomb`... bằng ResourceId, không thêm
EResource riêng tại host.

Catalog icon nhỏ của HUD có thể dùng reference trực tiếp. Không buộc tất cả icon
HUD tải async chỉ vì đã có Addressables. Nếu holder xuất hiện trước wallet,
OnEnable hiện tại không tự chờ registry: host phải tạo UI sau Ready hoặc Bind
explicit. Đây là điều kiện cần giữ khi mở scene trực tiếp để debug.

TemplateSave.Coins còn tồn tại cho fixture demo save/load và chưa được migrate.
Không sử dụng trường này làm wallet thứ hai. StartingCoins trong TemplateConfig
hiện phục vụ demo config/score, không tự seed Economy wallet. Cần fixture seed
riêng cho virtual-currency purchase; không cấp tiền lại sau mỗi lần khởi động.

## 5. Shop: chọn feature Shop

Trước sửa, HomeNavigator dùng ShopPanel của feature; FoundationDemoRoot gọi
UIShopPanel cũ qua cùng `Panel/ShopPanel.prefab`. Address này đang trỏ prefab
feature Shop, nên generic type của nhánh demo cũ không còn đúng.

Đã chuyển FoundationDemoRoot sang ShopPanel + ShopPresenter, Render trước
Transition để tạo item trước tween. Dispose presenter và tháo callback khi hide
hoặc destroy. Không còn UIShopPanel/OfferConfig/EResourceOffer riêng. HomeNavigator
được giữ vì là navigation glue của host, không phải logic giao dịch.

Lưu ý scene thực tế: prefab có tên FoundationDemo trong MainScene hiện gắn
HomeBootstrap, không gắn FoundationDemoRoot. Nhánh FoundationDemoRoot vừa sửa là
source demo dự phòng, chưa chứng minh được chạy qua scene hiện tại. Giữ source
FoundationDemoPanel/TemplateSave để phục vụ demo DataConfig/Datasave; phase 1
phải đưa chúng vào scene fixture riêng hoặc bỏ cả cụm nếu team không cần demo này.

Giữ UI integration trong `_Feature/Dreamy Shop/0.1.1/Shop Feature`: prefab đang
được address explicit, ShopOfferItem có EnsureTweenDelaySlot, panel áp stagger
khi thêm item. Không ghi sửa vào Library/PackageCache. Imported sample đã tùy
biến là code của host; re-import sample phải review diff, không overwrite mù.

Cần sửa tiếp nhưng chưa nằm trong lần cleanup này:

- ShopOfferItem chỉ hiển thị Rewards[0]; bundle nhiều reward đang bị thiếu trên UI.
- TitleKey đang hiện như chữ thô; thêm mapping/localization ở presentation.
- EnsureItems chỉ thêm item, không ẩn item dư khi catalog co lại.
- Presenter package có async purchase không gắn lifecycle cancellation của view;
  thử đóng panel trong khi gateway chờ, kiểm tra callback vào view bị destroy.
- Confirm IAP bằng gateway/receipt thật, transaction ID ổn định, không lấy GUID
  ngẫu nhiên của simulator làm mô hình production.
- Hai entry point Home/demo cần kiểm thử cùng scene để tránh hai presenter bind
  vào một panel được PanelManager trả lại. Tách demo scene là hướng nên làm.

## 6. Danh sách đã dọn

18 file gốc dưới đây đã được xóa cùng `.meta` tương ứng:

| Nhóm | File |
| --- | --- |
| Shop UI cũ | Scripts/UI/UIShopPanel.cs, UIResourceOfferItem.cs, UIGoldOfferItem.cs, UIGemOfferItem.cs, UIResourceValue.cs |
| Prefab Shop cũ | Prefabs/Panel/UIShopPanel.prefab; Prefabs/UI/UIResourceOfferItem.prefab, UIShopResourceOffer_Gold.prefab, UIShopResourceOffer_Gem.prefab |
| Config cũ/rỗng | Scripts/Config/OfferConfig.cs, RemoteDataConfigProvider.cs; Assets/Resources/DataConfig/offerConfigs.json |
| Save không có caller | Scripts/Save/NewSaveData.cs |
| Controller Settings sample không dùng | SettingsController.cs, RateUsDemo.cs, SettingsSampleInstaller.cs, SimulatedSettingsPlatformGateway.cs trong Settings Feature |
| Controller Shop sample không dùng | ShopDemo.cs trong Shop Feature |

Kiểm tra trước xóa: caller C#, GUID trong Assets/ProjectSettings, prefab variants
và group Addressables. Các item Shop cũ chỉ được prefab Shop cũ tham chiếu. QuickNav
có history trỏ prefab Shop cũ; các entry đó đã được bỏ để không giữ tham chiếu chết.
Folder address Panel/UI tự thu thập asset còn tồn tại sau refresh, không cần giữ
entry cho prefab đã xóa.

SettingsController/RateUsDemo không được hai prefab Settings/RateUs gắn component;
launcher mới đã là owner của presenter. Bỏ RateUsDemo cũng loại đoạn reward mẫu
cấp lại theo rating với transaction GUID mới. Nếu game cần reward, phải thiết kế
rule/claim state riêng, không đưa sample này trở lại luồng chính.

Bỏ address DailyRewardPanel không có consumer/prefab trong host hiện tại và hai
address item Shop cũ. Giữ ShopOfferAtlas để có thể làm demo asset loading riêng.
README gốc và README Settings đã được chỉnh để không hướng dẫn API/file vừa xóa.

Bản sao các file xóa trước cleanup nằm ở thư mục tạm Windows
`%TEMP%/dreamy-host-audit-20261004/Assets/...`. Không sửa index git hoặc commit;
workspace vốn đã có nhiều thay đổi staged của team.

## 7. Có nên giữ load sprite qua Addressables từ atlas?

Có, giữ như một demo năng lực Assets độc lập. Không giữ Shop cũ chỉ để minh họa
LoadSprite. Với HUD cố định vài icon, serialized reference/catalog đơn giản hơn.
Với catalog icon lớn, nội dung tải theo feature hoặc cập nhật từ xa, atlas có
owner rõ ràng và load theo nhu cầu là hợp lý.

Bằng chứng source phiên bản hiện tại:

- AssetLoader.LoadAsync cache theo type/path và chia sẻ request đang tải.
- LoadSprite gọi LoadAsync<SpriteAtlas> rồi atlas.GetSprite(spriteName).
- Unload<SpriteAtlas>(address) gỡ request/cache và release loader handle.
- API này không ref-count theo từng UI consumer; hai panel dùng chung atlas không
  được tự unload độc lập khi một panel đóng.
- LoadSprite catch lỗi và trả null; demo phải coi null là lỗi hiển thị cần xử lý.
- Loader không cache sprite clone lấy từ GetSprite theo tên. Owner demo phải
  quản lý clone, không gọi lại mỗi Render rồi bỏ quên object đã lấy.

Atlas hiện có một packable là folder `Textures/ShopOffer`, address folder
`SpriteAtlas` trong Default Local Group, và constant
`SpriteAtlas/ShopOfferAtlas.spriteatlasv2`. Atlas meta: max 2048, mipmap/readable
đều tắt, Android override đang bật. Không thể suy ra packed texture/bundle size
chỉ từ PNG source; cần build report. Không xóa PNG nguồn còn thuộc packables.

### Thiết kế demo nên làm ở phase 3

Tạo một scene/demo panel Assets riêng, không có wallet hay purchase:

1. Chỉ giữ vài icon đại diện với tên dễ hiểu; tách packable folder riêng nếu cần.
2. Đăng ký atlas address explicit để dễ kiểm tra thay vì dựa hoàn toàn folder address.
3. Panel/owner tải atlas một lần khi mở, GetSprite cho từng tên và cache clone theo
   tên trong phạm vi demo. Dùng AssetLoader.LoadAsync<SpriteAtlas> để lộ owner;
   có thể minh họa LoadSprite ở trường hợp chỉ tải một sprite, cùng quy tắc cleanup.
4. Khi async hoàn tất, kiểm tra owner còn sống và request generation còn đúng;
   không gán kết quả cũ vào cell đã rebind. Không SetNativeSize vào item layout
   cố định; dùng preserveAspect và kích thước layout.
5. Khi đóng, bỏ reference Image.sprite, Destroy các sprite clone do owner tạo,
   rồi Unload<SpriteAtlas> nếu không còn consumer. Nếu demo panel cache, chọn rõ
   giữ atlas cho lifetime cache hoặc giải phóng khi hide và reload khi show.
6. Không dùng UnloadAll khi đóng demo vì có thể phá asset của panel/audio khác.
7. Thử tên không tồn tại, đóng lúc tải, mở hai consumer cùng atlas, mở/đóng 20 lần.
   Đo memory/ref-count ổn định; không chỉ dựa vào console không lỗi.
8. Chạy cả Use Asset Database và build Addressables thật rồi player. Editor-fast
   có thể che lỗi key/bundle/dependency.

Unity hướng dẫn tránh duplicate asset giữa built-in scene, Resources và bundles,
và giữ cân bằng load/release. Atlas có cách xử lý dependency đặc thù; dùng Build
Layout/Analyze thay vì đoán rằng chuyển atlas sang Addressables luôn giảm size.
Nguồn: [Sprite atlases](https://docs.unity.cn/Packages/com.unity.addressables@1.22/manual/AddressablesAndSpriteAtlases.html),
[Memory management](https://docs.unity.cn/Packages/com.unity.addressables@1.17/manual/MemoryManagement.html).
Các trang là tài liệu nguyên tắc phiên bản cũ; đối chiếu tiếp Documentation~ của
Addressables 2.9.1 đã resolve trước khi đổi pipeline build.

## 8. Có nên xóa sprite cho nhẹ?

Không xóa hàng loạt trong lần này. `Assets/_Project/Textures` hiện có 59 file
không phải .meta, tổng khoảng 5.8 MiB trên đĩa. Đây không phải RAM texture hay
player download size. Icon/background hiện đang được prefab, scene và atlas
tham chiếu. Xóa asset đang pack hoặc trực tiếp dùng sẽ làm mất UI.

Tách ba bài toán:

| Mục tiêu | Việc phải đo/đổi |
| --- | --- |
| repository nhẹ | file source không còn dùng, sample dư, kích thước ảnh nguồn |
| build nhẹ | Resources, addressable folder inclusion, dependency/bundle duplication, compression |
| RAM nhẹ | atlas dimensions, platform format, mipmap, readable, lifetime/load scope |

Candidate chỉ được xóa sau khi kiểm tra serialized GUID, code address/name,
Resources.Load, addressable folder/label, SpriteAtlas packables và variant chain.
Không thấy GUID trực tiếp chưa đủ kết luận unused. Xóa theo batch nhỏ, kèm .meta,
refresh/import, build lại content và mở các màn UI bị ảnh hưởng. Chưa có số đo
nào chứng minh texture size đang là bottleneck; giữ art đang dùng.

## 9. Các phase còn cần làm

| Phase | Trạng thái | Deliverable | Điều kiện hoàn tất |
| --- | --- | --- | --- |
| 0. Source audit + cleanup an toàn | đã sửa source; validation tĩnh/compile | installer theo nhóm, một Shop nghiệp vụ, bỏ code cũ, báo cáo này | compile, GUID scan; Play Mode còn cần chạy |
| 1. Lifecycle và phân tách demo | chưa làm | chống installer duplicate, cleanup theo owner, rollback partial init, demo scene riêng | bootstrap lặp, Domain Reload off, hủy khi config đang load, direct Main có thông báo rõ |
| 2. Economy/Shop fixture hoàn chỉnh | chưa làm | seed wallet first-save, full reward UI, title mapping, giảm catalog xử lý item dư | mua đủ/thiếu tiền, không double charge, multi-reward, đóng khi IAP chờ, holder cập nhật |
| 3. Assets atlas demo | mới có thiết kế ở mục 7 | panel assets riêng, address explicit, shared owner/lifetime, vài icon | missing key/name, rebind, close during load, 20 vòng open/close, player content thật |
| 4. Dọn art và build inclusion | chưa làm | inventory references, batch art dư, group feature, Build Layout trước/sau | không missing sprite, không duplicate texture ngoài chủ đích, có số đo size/RAM |
| 5. Compatibility và template release | chưa làm | canonical toolkit/registry, pin package set, tests/harness CI, platform adapters | bộ version nhất quán, Runtime không Editor refs, build Android/iOS và smoke QA đạt |

Thứ tự triển khai: 1 -> 2 -> 3 -> 4 -> 5. Nếu team chuẩn bị phát hành game, adapter
IAP/Settings thật và pin dependency phải được ưu tiên trước khi gọi release-ready.
Phase 0 không tự nhận các phase sau đã hoàn tất.

## 10. Validation của lần sửa này

- Compile host bằng Roslyn đi kèm Unity 6000.4.12f1, từ response file Bee hiện có,
  lọc file đã xóa và đổi output sang thư mục tạm: nhánh UNITY_EDITOR exit 0.
- Compile cùng host bỏ define UNITY_EDITOR/UNITY_EDITOR_*: exit 0. Đây là kiểm tra
  nhánh tiền xử lý player bằng references Editor hiện có, không phải build player.
- Output compile nằm ngoài Library/ScriptAssemblies; không thay DLL Editor đang dùng.
- GUID scan của 18 asset/script đã xóa: không còn reference trong Assets/ProjectSettings.
- Parse 3 JSON DataConfig còn lại: PASS. Boundary 3 asmdef Runtime của host:
  không có reference Editor; source host không có using UnityEditor. git diff --check: PASS.
- Compile lại Settings Integration và Shop Integration sau xóa controller: exit 0.
  Warning có sẵn CS0108 tại RateUsPanel.SetInteractable(bool) che UIPanel.SetInteractable(bool),
  không phát sinh từ cleanup; chưa đổi API này.
- Không thêm test chỉ để lặp lại cấu trúc installer.
- Không có Unity MCP tool trong session. Endpoint bridge cục bộ 127.0.0.1:8080/mcp
  không kết nối được. Unity đang mở project; không chạy batchmode cạnh tranh hoặc
  đóng Editor của team để ép kiểm thử.
- Chưa refresh/đọc Console hiện tại qua Editor, chưa quan sát Play Mode, chưa build
  Addressables/player và chưa profile thiết bị. Không coi compile source là manual QA.

QA cần chạy trong Editor sau refresh: BootstrapScene -> Main -> Shop -> Close ->
Shop lần hai; Settings -> music/sfx -> RateUs -> Back; demo Foundation -> Shop;
IAP simulator Editor tăng wallet/holder đúng một lần; player không adapter trả
Unavailable; virtual purchase với fixture đủ tiền; pause/resume/restart giữ save;
không missing script/sprite và không callback lặp sau nhiều vòng mở panel.
## Cập nhật 2026-10-05: mở Foundation demo từ Home

HomePanel có field `demoButton`. HomeNavigator tạo FoundationDemoPanel, tạo
FoundationDemoRoot (controller C# thuần), bind/render trước Transition. Demo
không tự mở khi startup và không mở Shop hoặc panel khác. Giữ AddScore,
Damage/Heal, Save/Load và Close. Load hiện đọc TemplateSave và phục hồi score
đã lưu; config StartingCoins được dùng để khởi tạo demo khi mở.

Controller dispose callback khi panel hide/destroy hoặc navigator bị dispose.
Shop/Settings trên Home vẫn là các entry point chính riêng biệt.

Thiết lập thủ công trong Editor:

1. Thêm nút Demo vào HomePanel prefab và kéo vào field Demo Button.
2. Trong FoundationDemoPanel prefab gán labels, Add Score/Damage/Heal/Save/Load
   và Close Button. Bỏ các nút Shop/DailyReward mẫu còn trong prefab cũ.
3. Đảm bảo address `Panel/FoundationDemoPanel.prefab` trỏ đúng prefab có component
   FoundationDemoPanel. Không gắn FoundationDemoRoot vào GameObject: lớp này
   được navigator tạo khi mở, không còn là MonoBehaviour.
4. Không cần cấu hình OnClick thủ công; HomePanel tự đăng ký listener.
5. Chạy từ BootstrapScene; thử mở/Close/Back/mở lại, Save -> AddScore -> Load.

Không sửa scene/prefab trong lượt này theo yêu cầu team tự setup Editor.
Demo Button chỉ hiển thị trong Editor/Development Build. Compile Roslyn host
UNITY_EDITOR và nhánh bỏ define Editor đều exit 0; diff whitespace đạt.
Play Mode chưa chạy vì prefab/button do team tự setup sau lượt code này.

## Cập nhật adapter Settings/Rate Us (2026-10-05)

Host có SettingsPlatformGateway tại Scripts/Settings, được đăng ký trước
SettingsInstaller. Adapter dùng chung cho GDPR/Restore/Open Store/RequestReview.
Chưa có DreamySDK: capability=false và trả Unavailable; không giả thành công.
Hướng dẫn nối SDK nằm ở Assets/_Project/Scripts/Settings/README.md.

## Cập nhật lifecycle và demo save (2026-10-06)

Các thay đổi dưới đây thay thế mô tả cũ về teardown của GameInstaller và tên
field demo; không đánh dấu toàn bộ phase 1/2 đã hoàn tất.

- GameInstaller chỉ cho một instance cài service. Instance duplicate không
  teardown service của instance chính. Static owner/state/exception được reset
  tại SubsystemRegistration.
- Host ghi lại từng registration và cleanup theo thứ tự ngược khi init lỗi hoặc
  installer bị destroy. Chỉ unregister khi locator vẫn chứa đúng instance do
  host đăng ký; giữ nguyên replacement của owner khác. Khi undo registration,
  phục hồi registration có trước nếu nó chưa bị thay thế. Chỉ Dispose instance
  host tạo và sở hữu; không Dispose DreamyAudio.Service hay gateway có sẵn.
  Entitlement bindings được tháo trước service teardown, kể cả partial init.
- HomeNavigator kiểm tra disposed sau khi Create Shop hoàn tất, trước khi bind
  presenter. Nếu bind/render/transition lỗi, presenter và callback được tháo.
  Panel được Create vẫn thuộc cache của PanelManager, không bị navigator destroy.
- SceneLoader nhận CancellationToken tùy chọn và liên kết lifetime của loader.
  Event subscription tồn tại qua nhiều lần load; subscriber phải tự unsubscribe.
  Request trong khi loader đang bận bị từ chối bằng InvalidOperationException.
  Khi cancel hoặc lỗi callback sau khi Unity đã bắt đầu load, loader bật lại
  allowSceneActivation và chờ operation hoàn tất trước khi nhả trạng thái bận.
  Cancellation dừng presentation; không đảo ngược scene load của Unity, nên
  scene vẫn có thể được activate. OnScenePresented không được phát cho lượt hủy.
  GameInit quan sát cancellation/exception; scene load dùng lifetime của loader
  vì activation sẽ destroy GameInit trong scene bootstrap.
- TemplateConfig dùng StartingScore; JSON startingCoins cũ vẫn được đọc qua alias.
  TemplateSave version 2 giữ type và save key TemplateSave, dùng DemoOpenCount,
  CurrentScore, BestScore và IsInitialized. Migration version 1 chuyển LaunchCount
  thành DemoOpenCount, Coins thành CurrentScore, Score thành BestScore. Các key
  cũ chỉ được đọc, không ghi vào save mới. Score bằng 0 của save cũ được giữ.
  Demo seed score từ config một lần cho save mới; mở lại dùng score đã lưu và
  tăng DemoOpenCount. Save/Load thao tác score demo, không thao tác Economy wallet.

Validation:

- Roslyn host UNITY_EDITOR và nhánh bỏ define UNITY_EDITOR đều exit 0, output
  trong /tmp; dùng references Bee hiện có, không thay DLL Editor và không phải
  build player.
- Năm ca save/config chạy đạt ngoài Editor với source Datasave và Newtonsoft đã
  resolve: migration v1 với score 42/0, round-trip v2, save mới chưa seed, config
  tên cũ/mới. Harness chỉ thay Application.persistentDataPath và Debug output.
- EditMode tests nằm trong Assets/_Project/Tests/Editor: save/config và ownership
  registration (replacement, restore registration trước, borrowed service).
  Batch test bị Unity từ chối vì project đang được instance khác mở; ownership
  tests và Play Mode chưa được chạy trong Editor.

Smoke checks còn cần chạy trong Editor:

1. Hai installer trong cùng phiên; destroy duplicate không đổi locator/state.
2. Play Mode nhiều lần với Domain Reload off; kiểm tra riêng cấu hình Scene
   Reload off nếu sử dụng, vì reset static không tự khởi động lại scene giữ nguyên.
3. Hủy installer khi config đang tải và init lỗi sau partial installation;
   registry/binding/pool không còn instance mồ côi.
4. Dispose navigator lúc Create Shop đang chờ; lỗi transition; mở/đóng lại không
   tăng số callback.
5. Load scene hai lần với một subscriber; cancel khi progress dưới 0.9 hoặc đang
   chờ minimumLoadingDuration; callback ném exception; destroy loader khi tải.
   Operation phải được cho activate và nhả lock, không callback presentation muộn.
6. Demo first-save, Save -> đổi score -> Load, đóng/mở lại; save v1 có score 0;
   xác nhận Economy wallet không đổi.
