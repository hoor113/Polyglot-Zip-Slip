# File Upload / Zip Slip - Java Spring Boot

Spring Boot implementation with the same frontend and `/upload_bundle` contract. `ZipInputStream` deliberately writes normalized entries without checking that the result remains below the upload directory.

Run with `mvn spring-boot:run`, or build the included Dockerfile. The default port is `8080`.
