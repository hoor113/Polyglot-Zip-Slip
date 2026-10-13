# File Upload / Zip Slip - Node.js

Express implementation of the same lab UI and contract as the Django sample.

- `GET /upload_bundle` renders the upload page.
- `POST /upload_bundle` accepts the `bundle` multipart field and extracts ZIP entries.
- The extraction intentionally does not enforce a destination-prefix check.

Run locally with `npm install && npm start`, or build `Dockerfile`. The default port is `3000`.

## Safe Zip Slip PoC

Generate a harmless archive whose entry traverses one directory above the extraction
directory and writes only `zip-slip-proof.txt`:

```powershell
node .\create-payload.js
```

With the server running, upload it using the form or:

```powershell
curl.exe -F "bundle=@.\zip-slip-payload.zip" http://localhost:3000/upload_bundle
```

The extraction log should show a resolved destination ending in
`zip-slip-proof.txt` outside `UPLOAD_DIR`. If `UPLOAD_DIR` is not set, the
default Linux path is `/var/lib/zip-slip/temp_extract`; set a writable local
directory when running directly on Windows, for example:

```powershell
$env:UPLOAD_DIR = "$PWD\temp_extract"
npm start
```

The marker will then be created at `.\zip-slip-proof.txt`, next to the configured
extraction directory. Use this PoC only against the local lab instance.
