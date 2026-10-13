# File Upload / Zip Slip - Java Spring Boot

Spring Boot implementation with the same frontend and `/upload_bundle` contract. `ZipInputStream` deliberately writes normalized entries without checking that the result remains below the upload directory.

Run with `mvn spring-boot:run`, or build the included Dockerfile. The default port is `8080`.

Build the harmless proof-of-concept archive with:

```text
python build_payload_springboot.py
```

Upload the generated `exploit.zip` at `http://localhost:8080/upload_bundle`.
The traversal writes a marker to `/tmp/zip-slip-spring-boot-proof.txt`, outside
the configured extraction directory. This payload does not execute commands or
overwrite application files.
