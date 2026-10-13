# File Upload và Zip Slip

- Web A route: `POST /upload_bundle`.
- Kiểm tra extension `.zip` nhưng không canonicalize member path trước khi ghi file.
- Payload mẫu nằm trong `build_payload.py`; entry traversal mặc định ghi đè `attacks/04_ssti_rce/preview.html` để nối sang module SSTI.
- Runtime implementation: `server.py` và `templates/upload.html` trong module này.

Chạy tạo archive:

```powershell
python attacks/02_file_upload_zip_slip/build_payload.py
```
