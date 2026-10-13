# Polyglot Zip Slip

Lab này minh họa lỗ hổng **Zip Slip** trong nhiều công nghệ web khác nhau. Mỗi
module cung cấp cùng một giao diện upload ZIP và cùng hợp đồng HTTP để có thể
so sánh cách các thư viện xử lý đường dẫn bên trong archive.

> **Cảnh báo an toàn:** Đây là mã cố ý dễ bị khai thác và chỉ được chạy trong
> môi trường lab cô lập. Không upload archive tạo bởi lab này vào hệ thống
> production hoặc hệ thống mà bạn không có quyền kiểm thử. Các payload nên chỉ
> ghi marker vô hại trong thư mục tạm của lab.

## Zip Slip là gì?

Zip Slip xảy ra khi ứng dụng nối tên file trong ZIP vào thư mục giải nén mà
không kiểm tra đường dẫn sau khi canonicalize/normalize. Một entry như
`../marker.txt` có thể khiến file được ghi ra ngoài thư mục dự kiến.

Luồng dễ bị lỗi trong các module:

1. Nhận file multipart với field `bundle`.
2. Lưu archive vào thư mục tạm.
3. Duyệt từng entry trong ZIP.
4. Ghép `uploadDir` với tên entry và ghi file.
5. Hiển thị đường dẫn đích trong extraction log.

Điểm cố ý dễ bị khai thác là bước 4: code mẫu chưa xác nhận destination cuối
cùng vẫn nằm bên trong `uploadDir`.

## Các module

| Module | Công nghệ | Cổng mặc định | Lệnh chạy chính |
| --- | --- | ---: | --- |
| [`Python-Flask`](./Python-Flask/) | Flask, `zipfile` | `80` | `python server.py` |
| [`Java-SpringBoot`](./Java-SpringBoot/) | Spring Boot 3.4, Java 17 | `8080` | `mvn spring-boot:run` |
| [`JavaScript-Nodejs`](./JavaScript-Nodejs/) | Express, `adm-zip`, Multer | `3000` | `npm install && npm start` |
| [`PHP-Laravel`](./PHP-Laravel/) | Laravel 11, PHP 8.2+ | `8000` | `composer install && php artisan serve --port=8000` |
| [`CSharp-DotNet`](./CSharp-DotNet/) | ASP.NET Core, .NET 8 | `8080` | `dotnet run --urls http://localhost:8080` |

Mỗi module có README riêng với các yêu cầu và PoC tương ứng. Dockerfile cũng
được cung cấp trong các module để chạy lab trong container.

## Hợp đồng HTTP chung

Mở trang upload:

```text
GET /upload_bundle
```

Gửi archive:

```text
POST /upload_bundle
Content-Type: multipart/form-data
Field: bundle
```

Ví dụ với Node.js:

```powershell
curl.exe -F "bundle=@.\exploit.zip" http://localhost:3000/upload_bundle
```

Các triển khai trả về trang HTML chứa trạng thái xử lý và danh sách entry đã
được giải nén. `UPLOAD_DIR` có thể được đặt để thay đổi thư mục tạm; nên dùng
một thư mục riêng, có quyền tối thiểu và không chứa mã nguồn ứng dụng.

## Chạy một module

### Python Flask

```powershell
cd .\Python-Flask
python server.py
```

Tạo archive PoC theo module:

```powershell
python .\build_payload_django.py
```

### Java Spring Boot

Yêu cầu JDK 17 và Maven:

```powershell
cd .\Java-SpringBoot
mvn spring-boot:run
```

Tạo marker PoC:

```powershell
python .\build_payload_springboot.py
```

### Node.js

Yêu cầu Node.js và npm:

```powershell
cd .\JavaScript-Nodejs
npm install
npm start
```

Khi chạy trực tiếp trên Windows, đặt `UPLOAD_DIR` tới thư mục có thể ghi:

```powershell
$env:UPLOAD_DIR = "$PWD\temp_extract"
npm start
```

### Laravel

Yêu cầu PHP 8.2+, Composer và PHP extension `zip`:

```powershell
cd .\PHP-Laravel
composer install
php artisan serve --port=8000
```

### ASP.NET Core

Yêu cầu .NET SDK 8:

```powershell
cd .\CSharp-DotNet
dotnet run --urls http://localhost:8080
```

Tạo archive marker bằng project `PayloadBuilder`:

```powershell
dotnet run --project .\PayloadBuilder
```

## Chạy bằng Docker

Mỗi module có Dockerfile độc lập. Ví dụ với Flask:

```powershell
cd .\Python-Flask
docker build -t polyglot-zip-slip-python .
docker run --rm -p 8080:80 polyglot-zip-slip-python
```

Đối với module khác, thay thư mục và tag image tương ứng. Không bind-mount
thư mục mã nguồn hoặc thư mục nhạy cảm của máy host vào container lab.

## Kiểm thử và quan sát

1. Chỉ khởi chạy **một** module tại một thời điểm để tránh nhầm cổng.
2. Tạo PoC vô hại bằng payload builder của module.
3. Upload file qua UI hoặc `curl.exe`.
4. Kiểm tra extraction log và xác nhận marker chỉ được tạo trong môi trường
   lab đã chuẩn bị.
5. Dọn archive và marker sau khi kiểm thử.

Không sử dụng payload để thực thi lệnh, ghi đè file hệ thống, truy cập dữ liệu
người dùng hoặc kiểm thử ngoài phạm vi được ủy quyền.

## Khắc phục lỗ hổng

Khi giải nén, cần:

1. Lấy đường dẫn tuyệt đối/canonical của thư mục đích.
2. Chuẩn hóa tên entry và ghép với thư mục đích.
3. Kiểm tra destination bắt đầu bằng thư mục đích, có ranh giới thư mục.
4. Từ chối path tuyệt đối, path traversal (`..`) và symlink nguy hiểm.
5. Giới hạn kích thước archive, số entry và kích thước sau giải nén.
6. Giải nén bằng user không có quyền ghi ra ngoài workspace tạm.

Mẫu kiểm tra khái quát:

```text
root = canonical(uploadDir)
target = canonical(join(root, archiveEntryName))
if target is not root and target is not below root:
    reject("Directory traversal detected")
```

Chỉ kiểm tra chuỗi bằng `startswith` là chưa đủ nếu không bảo đảm ranh giới
thư mục; ví dụ `/tmp/extract-other` không được coi là con của
`/tmp/extract`.

## Cấu trúc thư mục

```text
Zip-Slip/
├── Python-Flask/
├── Java-SpringBoot/
├── JavaScript-Nodejs/
├── PHP-Laravel/
├── CSharp-DotNet/
└── README.md
```

`node_modules/`, `target/`, `bin/` và `obj/` là artefact phụ thuộc/build của
 từng module, không phải thành phần cần thiết để hiểu luồng khai thác.

