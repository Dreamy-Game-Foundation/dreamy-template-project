# Changelog

Lịch sử thay đổi tích hợp của Dreamy Template. Changelog của từng package nằm trong source package tương ứng.

## 2026-11-17

### Cập nhật toàn bộ package Dreamy đang cài

- Đối chiếu tag và HEAD trên remote của cả 13 package đang dùng. Nâng Debugger từ 0.1.4 lên tag 0.1.7, commit `1e5fd6ed45861ac2c451d3480cd81a4063fd17d0`.
- Debugger nhận Screenshot mode, các luồng capture và manual Hide UI độc lập, khôi phục UI, sửa safe area và surface/access UI. Dependency, GUID và schema save không đổi. Giữ cấu hình template RequiredTaps = 5, PreferenceRevision = 1 và các tính năng upload/report đang tắt; không reset dữ liệu hoặc import lại sample.
- Core giữ version 1.2.0, chuyển sang commit `a491b74282456ddf7a707f5bf4e31f08c589627d` mới nhất để nhận sửa chú thích lịch sử API; runtime không đổi. Đồng bộ changelog Shop 0.3.0 với commit `3822cbaa6515168ff3fb9fa7254a4818d55cc736`.
- Sáu package embedded khớp source HEAD trên remote: Editor Tools 1.2.0 (`074aa33bd982`), Feature 0.2.0 (`fec5ceeb8509`), Economy 0.8.1 (`2ed43e881a87`), Settings 0.3.0 (`4eeb5cbe2a46`), Shop 0.3.0 (`3822cbaa6515`) và UI 1.1.0 (`b2db2683bc97`).
- Assets 1.0.1, Audio 1.0.5, DataConfig/Datasave 1.0.1 và Feedback 0.4.1 đã ở bản mới nhất; giữ nguyên source. Không thêm package mới; Localization chưa tích hợp và UI ngôn ngữ Settings vẫn ẩn.
- Bật testables cho Debugger trong manifest. Compile sạch; 34 test EditMode template, 61 test EditMode Debugger và 37 test PlayMode Screenshot/startup/preferences đều pass tại Unity 6000.4.12f1.
- Smoke từ BootstrapScene đến MainScene: bootstrap Ready, Debugger 0.1.7 chạy và đăng ký đủ cheat template; RequiredTaps = 5, PreferenceRevision = 1 giữ nguyên; Console không có error.
- Cập nhật README theo nguồn package hiện tại, bỏ tham chiếu hiện hành tới evidence/compatibility đã được xóa. Chưa build APK/AAB hoặc kiểm chứng native screenshot/gallery/share trên thiết bị trong đợt này.

## 2026-11-04

### Sửa layout resource holder

- Gỡ 180 override Rect cũ trong EconomyHolder, HomePanel và ShopPanel, cùng 38 override tại instance Home trong UIPortraitSandbox. Các thành phần Icon, DisplayName và Amount kế thừa lại layout của prefab nền sau khi package chuyển từ HorizontalLayoutGroup sang anchor/offset trực tiếp.
- Giữ vị trí root cần thiết, layout có chủ đích và các binding đang dùng. Khi lưu, Unity cũng bỏ bốn modification cũ trỏ tới object/component nguồn không còn tồn tại.
- Rà 16 prefab: không còn nhóm override Rect con bằng 0 từ layout cũ hoặc override Rect trỏ tới target đã mất. Kiểm tra 27 Rect con của holder qua sáu prefab khớp prefab nền; icon/text còn binding. Mở lại UIPortraitSandbox để kiểm tra các instance đã lưu.
- Cập nhật README về migration layout và override tại các lớp prefab/scene. Chưa chạy lại PlayMode hoặc build sau lần sửa layout này; các test ngày 2026-11-03 không chứng minh kết quả hiển thị của lần sửa này.

Hồ sơ: [checkpoint](docs/evidence/rect-override-revert-2026-11-04/checkpoint.json), [prefab verification](docs/evidence/rect-override-revert-2026-11-04/verification.json), [scene verification](docs/evidence/rect-override-revert-2026-11-04/scene-verification.json).

## 2026-11-03

### Cập nhật package và integration

- Nâng Feature lên 0.2.0, UI lên 1.1.0, Economy lên 0.8.0, Shop lên 0.3.0 và Settings lên source 0.3.0; cập nhật source Editor Tools có shortcut Clear Save Data.
- Đồng bộ version Assets 1.0.1, Audio 1.0.3, DataConfig 1.0.1 và Datasave 1.0.1. Các lần tăng version này so với baseline chỉ đổi metadata/tài liệu.
- Lưu sáu snapshot source trong Packages với commit nguồn cụ thể vì một số revision chưa có trên Git remote. Giữ nguyên tập package đã có; không thêm Localization. Settings không nhận language gateway và giữ UI ngôn ngữ ẩn.
- Cập nhật source sample Shop/Settings tại các đường dẫn import lịch sử, giữ GUID. Bổ sung popup thiếu Coin/Gem qua shared presenter factory và Addressables; đóng popup trước khi chuyển tới offer, không tự mua hoặc retry khi trở về.
- Shop trì hoãn refresh reward HUD trước khi wallet thay đổi, đợi icon flight và giải phóng scope khi hoàn tất, hủy hoặc đóng view. Bật auto-refresh cho holder và bổ sung preset MoveFade/ScaleFade vào thư viện của template.
- Kiểm tra wallet save v1 lên schema v4, bảo toàn balances/entitlement/transaction IDs và chống grant lặp sau restart. Sửa reference asmdef Wallet Demo từ Unity.UGUI sang UnityEngine.UI.

### Kiểm chứng

- Pass 121 test EditMode package/template trong lượt đầu; bộ template cuối pass 33 test, thay thế 29 test template trong lượt đầu. Bộ tween UI pass 28 test.
- Pass 19 test PlayMode holder và ba test dùng prefab Shop với icon flight thật, bao gồm cleanup khi hide/dispose.
- Build Addressables cho Android thành công. Smoke Bootstrap → Home → Settings/Shop, popup thiếu Gem, Later, mở lại và chuyển tới offer; số dư không đổi trong luồng thiếu tài nguyên.
- Chưa build APK/AAB hoặc kiểm tra trên thiết bị. Các test binding/hành vi trong đợt này chưa phát hiện override Rect cũ, được sửa tiếp ngày 2026-11-04.

Hồ sơ: [summary](docs/evidence/dreamy-upgrade-2026-11-03/summary.json), [source commits](docs/evidence/dreamy-upgrade-2026-11-03/source-snapshots.json).
