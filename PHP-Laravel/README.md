# File Upload / Zip Slip - PHP Laravel

Laravel implementation with the same upload page and `/upload_bundle` contract. The `ZipArchive` extraction intentionally omits canonical destination validation.

Run with `composer install && php artisan serve --port=8000`, or build the included Dockerfile. PHP's `zip` extension is required.
