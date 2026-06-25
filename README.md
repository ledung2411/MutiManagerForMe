# MutiManagerForMe

Ứng dụng Windows offline để quản lý công việc, ghi chú, lịch trình, nhắc việc và thu chi cá nhân trong một nơi.

## Trạng thái

MVP hiện có:

- Dashboard tổng hợp công việc, lịch, ghi chú ghim và tài chính tháng.
- Công việc với deadline, ưu tiên, nhắc giờ và lặp hàng ngày/tuần/tháng.
- Ghi chú có ghim, chỉnh sửa và lưu cục bộ.
- Lịch trình theo ngày với nhắc trước sự kiện.
- Thu/chi, nhiều ví, danh mục và ngân sách tháng.
- Thông báo nền qua khay hệ thống khi đóng cửa sổ chính.
- Sao lưu database từ màn hình Cài đặt.

Ứng dụng lưu dữ liệu tại:

```text
%LOCALAPPDATA%\MutiManagerForMe\mutimanager.db
```

## Công nghệ

- C# và WPF trên .NET 10.
- MVVM với CommunityToolkit.Mvvm.
- SQLite với Microsoft.Data.Sqlite.
- xUnit cho kiểm thử tầng dữ liệu.

Các quyết định UI/UX và phạm vi sản phẩm nằm trong [DESIGN.md](DESIGN.md).

## Chạy dự án

Yêu cầu Windows 10/11 và .NET 10 SDK.

```powershell
dotnet restore
dotnet build MutiManagerForMe.slnx
dotnet run --project src/MutiManagerForMe.App/MutiManagerForMe.App.csproj
```

Khi nhấn nút đóng cửa sổ, ứng dụng tiếp tục chạy trong khay hệ thống để phát nhắc việc. Nhấp đúp biểu tượng khay để mở lại hoặc chọn **Thoát** trong menu chuột phải để tắt hoàn toàn.

## Kiểm thử

```powershell
dotnet test MutiManagerForMe.slnx
```

## Phạm vi tiếp theo

- Tìm kiếm và bộ lọc toàn cục.
- Chỉnh sửa task và lịch đã tạo.
- Lịch tuần/tháng và cảnh báo trùng lịch.
- Giao dịch định kỳ, chuyển tiền giữa các ví và báo cáo biểu đồ.
- Khôi phục backup, xuất CSV/Excel và mã hóa dữ liệu cục bộ.
- Đóng gói MSIX/installer và tùy chọn khởi động cùng Windows.
