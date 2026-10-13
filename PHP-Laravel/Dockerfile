FROM composer:2.8 AS build
WORKDIR /app

# Copy the project files
COPY . .

# Install dependencies WITHOUT running scripts yet
RUN composer install --no-dev --no-interaction --prefer-dist --no-progress --no-scripts
RUN mkdir -p bootstrap/cache && chmod -R 775 bootstrap/cache
RUN composer dump-autoload --optimize

FROM php:8.4-cli
WORKDIR /app

# This demo does not require a database.
ENV SESSION_DRIVER=file \
    CACHE_STORE=file

# Install system dependencies required for the zip extension
RUN apt-get update && apt-get install -y \
    libzip-dev \
    && docker-php-ext-install zip \
    && rm -rf /var/lib/apt/lists/*

# Copy built vendor files and app code from the build stage
COPY --from=build /app .

# Run package discovery in the final PHP environment where extensions exist
RUN php artisan package:discover --ansi

# Set up storage permissions and non-root user
RUN mkdir -p \
    storage/app/temp_extract \
    storage/framework/cache \
    storage/framework/sessions \
    storage/framework/views \
    storage/logs \
    bootstrap/cache \
    && useradd -u 1001 -m appuser \
    && chown -R appuser:appuser /app
USER appuser

EXPOSE 8000
CMD ["php", "artisan", "serve", "--host=0.0.0.0", "--port=8000"]