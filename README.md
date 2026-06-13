# WebDocTruyen

WebDocTruyen là một dự án .NET được xây dựng theo kiến trúc nhiều lớp (Application, Domain, Infrastructure, Web, API).  
Mục tiêu của dự án là phát triển một nền tảng đọc truyện trực tuyến với khả năng mở rộng, dễ bảo trì và tích hợp API.

## 📂 Cấu trúc dự án
- `.vs/` : Thư mục cấu hình Visual Studio
- `WebDocTruyen.Application/` : Lớp xử lý logic nghiệp vụ
- `WebDocTruyen.Domain/` : Lớp định nghĩa các thực thể và quy tắc miền
- `WebDocTruyen.Infrastructure/` : Lớp hạ tầng, kết nối cơ sở dữ liệu, repository
- `WebDocTruyen.Web/` : Giao diện web
- `WebDocTruyen.api/` : API phục vụ cho ứng dụng khác
- `WebDocTruyen.sln` : File solution của Visual Studio

## 🚀 Cách chạy dự án
1. Clone repo:
   ```bash
   git clone https://github.com/tuan13052005/WebDocTruyen.git
