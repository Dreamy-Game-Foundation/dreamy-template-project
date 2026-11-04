# Dreamy Feature

Contract C# và vòng đời presenter dùng chung cho các tính năng game.
`Dreamy.Feature.Runtime` không phụ thuộc package/assembly khác và giữ
`noEngineReferences=true`. Config, save, giao thưởng và điều hướng UI thuộc
các package sở hữu chức năng tương ứng.

## API runtime

| Kiểu | Trách nhiệm |
| --- | --- |
| `FeatureId` | ID tính năng không rỗng, so sánh chuỗi theo ordinal. |
| `FeatureState` | Các trạng thái Locked / Available / Active / Completed. |
| `FeatureMetadata` | Ghép ID và trạng thái; game sở hữu trạng thái và việc lưu. |
| `IFeatureView<TState>` | Hiển thị trạng thái có kiểu cụ thể. |
| `IFeaturePresenter` | Contract Show / Refresh hiện có, được giữ nguyên. |
| `FeaturePresenter` | Vòng đời bind / refresh / unbind; triển khai IFeaturePresenter và IDisposable. |

Giữ các contract ID/state/metadata cho consumer cần dùng; không bắt buộc sử dụng
chúng khi kế thừa vòng đời presenter.

## Vòng đời presenter

```csharp
public sealed class ExamplePresenter : FeaturePresenter
{
    private readonly IFeatureView<ExampleState> view;
    private readonly ExampleService service;

    public ExamplePresenter(IFeatureView<ExampleState> view, ExampleService service)
    {
        this.view = view;
        this.service = service;
    }

    protected override void Bind() => service.Changed += Refresh;
    protected override void Unbind() => service.Changed -= Refresh;
    public override void Refresh() => view.Render(service.GetState());
}
```

`ExampleState` và `ExampleService` do game cung cấp. Show chỉ bind một lần nhưng
Refresh ở mỗi lần gọi. Dispose tháo subscription một lần; Show sau đó có thể bind
lại. Nếu Bind hoặc Refresh trong Show lỗi, subscription được giải phóng trước khi
ném lại lỗi. Unbind phải chịu được Bind mới hoàn tất một phần và không được throw.
Các lệnh đồng bộ và do cùng một thread sở hữu.

Vòng đời này chỉ quản lý subscription, không huỷ tác vụ async, chặn callback muộn,
điều phối giao dịch hoặc sở hữu service. Tính năng có async vẫn tự quản lý
cancellation, generation guard và trạng thái nghiệp vụ.

DailyRewardPresenter và MissionPresenter dùng base này; các view kế thừa
IFeatureView<TState>. Presenter dùng với UI triển khai thêm Dreamy.UI.IPanelPresenter
trong assembly của mình. Feature không tham chiếu UI; factory/host của UI quản lý
vòng đời mỗi lần mở panel.

## Cài đặt và chiều dependency

Cài package local hoặc repository qua Unity Package Manager. Thêm
`Dreamy.Feature.Runtime` vào mọi asmdef dùng trực tiếp các kiểu của package, kể cả
view kế thừa interface của tính năng dựa trên IFeatureView<TState>.
Daily/Missions tự khai báo DataConfig/Datasave/Economy/UI theo nhu cầu.
Feature không cần DataConfig.

Package không tạo installer/service bootstrap, service locator, save chung,
bảng config, registry hay framework reward tổng quát.

## Prefab có sẵn

Giữ nguyên đường dẫn và GUID của `Runtime/Prefabs/BaseFeaturePanel.prefab` và
`BaseFeatureItem.prefab` để các prefab variant hiện có không mất reference.
Các asset hiển thị tuỳ chọn này tham chiếu component Dreamy UI, nên project sử dụng
chúng cần cài Dreamy UI và dependency tương ứng. Assembly C# runtime vẫn độc lập
với các asset đó và UnityEngine.

Tạo variant của game, giữ GUID nền, rồi gán panel, button và presenter factory.
Điều hướng, key Addressables và safe area do phần tích hợp UI của game quản lý.

## Kiểm chứng và tương thích

Thêm `com.dreamy.feature` vào testables của manifest để chạy
`Dreamy.Feature.Runtime.Tests` ở EditMode. Các test bao phủ Show/Dispose lặp,
mở lại, lỗi bind một phần và cleanup khi render lỗi. Chạy thêm suite Daily/Missions
để kiểm chứng consumer thực tế.

Giữ contract Feature cũ, tên assembly, GUID `.meta` và đường dẫn prefab. Base mới
là API bổ sung; view Daily/Missions đã kế thừa IFeatureView<TState>, nên consumer
source cần reference Feature trực tiếp và compile lại. Không thay schema save.
Thông tin version phát hành nằm trong package.json và lịch sử tag; kết quả test
trên source không thay thế kiểm chứng consumer, build hoặc device.
