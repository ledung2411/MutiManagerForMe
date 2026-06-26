# List update chức năng

## v1.1 - WinUI 3 migration

- Chuyển giao diện desktop từ WPF sang WinUI 3 / Windows App SDK.
- Giữ kiến trúc MVVM, SQLite local-first và các module hiện có.
- Cập nhật publish/installer sang WinUI 3 self-contained win-x64.
- Sửa các label tiếng Việt chính để hiển thị Unicode đúng.
- Giữ nhắc việc qua system tray khi đóng cửa sổ chính.

## v1.2 - Hoàn thiện thao tác dữ liệu

- Sửa task đã tạo: tiêu đề, mô tả, hạn, nhắc việc, ưu tiên, lặp lại.
- Sửa lịch trình đã tạo: tên, mô tả, giờ bắt đầu/kết thúc, nhắc trước.
- Sửa giao dịch thu/chi đã nhập.
- Thêm xác nhận rõ hơn cho thao tác xóa nhiều dữ liệu.
- Thêm trạng thái empty/loading/error nhất quán cho từng màn.

## v1.3 - Tìm kiếm và lọc

- Tìm kiếm toàn cục theo công việc, ghi chú, lịch trình và giao dịch.
- Lọc task theo trạng thái, ưu tiên, quá hạn, hôm nay, tuần này.
- Lọc giao dịch theo ví, loại thu/chi, danh mục và khoảng ngày.
- Lọc ghi chú theo ghim, ngày cập nhật và từ khóa.

## v1.4 - Lịch và nhắc việc nâng cao

- Lịch tuần/tháng.
- Cảnh báo trùng lịch.
- Nhắc lại sau 5/10/30 phút.
- Tùy chọn khởi động cùng Windows.
- Trung tâm nhắc việc trong app để xem lại thông báo đã phát.

## v1.5 - Quản lý chi tiêu nâng cao

- Giao dịch định kỳ theo ngày/tuần/tháng.
- Chuyển tiền giữa các ví.
- Ngân sách theo danh mục, không chỉ tổng ngân sách tháng.
- Biểu đồ thu chi theo tháng và theo danh mục.
- Báo cáo dòng tiền và số dư từng ví.

## v1.6 - Backup, xuất dữ liệu và bảo mật

- Khôi phục từ file backup.
- Xuất CSV/Excel cho task, note, lịch, giao dịch.
- Mã hóa database local bằng mật khẩu.
- Tùy chọn tự động backup theo ngày/tuần.
- Kiểm tra dữ liệu lỗi trước khi backup/restore.

## v2.0 - Đồng bộ tùy chọn

- Đồng bộ qua Google Drive/OneDrive theo lựa chọn người dùng.
- Cơ chế conflict resolution khi sửa trên nhiều máy.
- Import dữ liệu từ file CSV/Excel.
- Hồ sơ người dùng cá nhân hóa màu, tiền tệ, định dạng ngày giờ.
