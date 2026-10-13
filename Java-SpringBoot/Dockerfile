FROM maven:3.9-eclipse-temurin-17 AS build
WORKDIR /src
COPY pom.xml .
RUN mvn -q dependency:go-offline
COPY src src
RUN mvn -q package -DskipTests
FROM eclipse-temurin:17-jre
WORKDIR /app
COPY --from=build /src/target/file-upload-zip-slip-1.0.0.jar app.jar
RUN mkdir -p /var/lib/zip-slip/temp_extract && useradd -u 1001 -m appuser && chown -R appuser:appuser /app /var/lib/zip-slip
USER appuser
EXPOSE 8080
ENTRYPOINT ["java", "-jar", "app.jar"]
