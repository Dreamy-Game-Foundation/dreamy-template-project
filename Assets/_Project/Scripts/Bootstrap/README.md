# Khởi tạo và tích hợp feature

`GameInstaller` là composition root của ứng dụng: tạo service, nối các feature và quản lý cleanup. Component tồn tại qua các scene nhờ `DontDestroyOnLoad`. Các file partial dưới đây thuộc cùng một component và cùng vòng đời:

| File | Trách nhiệm |
| --- | --- |
| `GameInstaller.cs` | Thứ tự bootstrap, trạng thái Ready/Failed, cancellation và hook lưu dữ liệu khi pause/quit. |
| `GameInstaller.Foundation.cs` | Datasave, pooling, audio, feedback, economy và config. |
| `GameInstaller.Features.cs` | Settings, Shop, callback billing và hiệu ứng entitlement của project. |
| `GameInstaller.Presentation.cs` | Presenter factory, gán factory cho manager của scene và điều hướng Rate Us. |
| `GameInstaller.Services.cs` | Theo dõi quyền sở hữu đăng ký service và cleanup theo thứ tự ngược. |

## Chạy project

Đặt một `GameInstaller` trong BootstrapScene; không đặt thêm installer ở các scene sau. Static owner guard từ chối component installer thứ hai trước khi đăng ký service. Destroy component bị từ chối không reset trạng thái hoặc dọn service của installer đang hoạt động.

`GameInit` và `HomeBootstrap` đọc trạng thái bootstrap. Khi tải lại BootstrapScene, installer trùng bị từ chối nhưng GameInit được giữ để luồng khởi động tiếp tục. `GameInit.Start` đặt mục tiêu 60 FPS trước khi chờ bootstrap.

Thứ tự khởi tạo: persistence/infrastructure → economy → đăng ký và tải config → tạo presenter factory → cài feature/presentation → Ready. Khi thất bại, đọc `InitializationException` và Console; GameInit không chuyển scene nếu trạng thái Failed.

## Thêm service hoặc feature

1. Đăng ký bảng config trong `InstallConfigurationAsync` trước bước load.
2. Tạo method cài feature riêng và gọi từ `InstallFeatures` khi dependency đã sẵn sàng.
3. Dùng `RegisterService` cho đăng ký trực tiếp. Chỉ đặt `ownsInstance` cho instance do project tạo và chịu trách nhiệm dispose.
4. Dùng `TrackFeatureInstallation` cho installer package tự đăng ký service.
5. Đăng ký presenter constructor trong factory; mở panel qua PanelManager.

Cleanup khôi phục đăng ký trước đó, giữ các đăng ký thay thế do owner khác cài và dispose instance thuộc project theo thứ tự ngược. Truyền dependency xuống presenter/component qua constructor hoặc Initialize; giữ việc resolve service ở root.

## MVP và vòng đời panel

View giữ reference Button/TMP/Image, phát event input và thực hiện render. Presenter subscribe event, gọi service/navigation và cập nhật View. Service giữ quy tắc và dữ liệu; View không tự thay đổi wallet hoặc ghi save.

`InstallPresentation` đăng ký Home/Demo và gán factory chung cho PanelManager của các scene. Settings/Shop đăng ký presenter qua feature installer. Manager tạo presenter khi mở panel và giải phóng khi đóng/disable/destroy hoặc mở lỗi; mở lại View cache sẽ tạo presenter mới. Không tạo presenter thứ hai trong lifecycle của View.

Presenter cần gỡ event và hủy tác vụ theo lần mở trong Dispose. Token OnDestroy của View không đủ khi panel chỉ đóng hoặc được cache. Giữ guard chống thao tác lặp, xử lý exception và kiểm tra dispose trước khi cập nhật View sau await.

Tham khảo `HomePresenter`, `HomePanel`, `HomeNavigator` và `ShopFeatureInstaller` để thêm feature theo cùng cách tổ chức.

Để test Shop, chạy BootstrapScene trong Editor rồi mua gói IAP. `Simulate Iap Purchases In Editor` trên GameInstaller mặc định bật: mỗi lần mua trả thành công với transaction ID riêng, Shop cấp thưởng qua wallet và lưu quyền sở hữu như bình thường. Gói consumable có thể mua nhiều lần; gói mua một lần sẽ chuyển sang đã sở hữu. Dữ liệu test được lưu vào save hiện tại. Tắt tùy chọn này để kiểm tra trường hợp billing chưa cấu hình. Gateway do host đăng ký sẵn vẫn được ưu tiên.

Giả lập chỉ được biên dịch trong Editor. Billing trên thiết bị trong `PurchaseWithBillingAsync` hiện còn TODO và trả thất bại; thay bằng tích hợp SDK/backend thật trước khi phát hành.

Xem [README chính](../../../../README.md) để biết cách cài project/package, sử dụng Addressables, UniTask, Sprite Atlas, asmdef và quy trình kiểm tra.
