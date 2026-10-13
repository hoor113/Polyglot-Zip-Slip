<?php

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;

function uploadPage(?string $message = null, string $status = 'info', array $extractedFiles = []): \Illuminate\Contracts\View\View
{
    return view('upload', compact('message', 'status', 'extractedFiles'));
}

Route::get('/upload_bundle', fn () => uploadPage());

Route::post('/upload_bundle', function (Request $request) {
    $bundle = $request->file('bundle');
    if (!$bundle || !$bundle->isValid()) {
        return uploadPage('No file selected', 'danger');
    }

    $uploadDir = realpath(storage_path('app')) . DIRECTORY_SEPARATOR . 'temp_extract';
    if (!is_dir($uploadDir)) {
        mkdir($uploadDir, 0777, true);
    }
    $archivePath = $uploadDir . DIRECTORY_SEPARATOR . 'uploaded_package.zip';
    $bundle->move($uploadDir, 'uploaded_package.zip');
    $archive = new ZipArchive();
    $extractedFiles = [];

    try {
        if ($archive->open($archivePath) !== true) {
            throw new RuntimeException('Unable to open archive');
        }
        for ($index = 0; $index < $archive->numFiles; $index++) {
            $entry = $archive->statIndex($index);
            $name = $entry['name'];
            // Deliberately vulnerable: no canonical destination-prefix validation.
            $target = $uploadDir . DIRECTORY_SEPARATOR . str_replace(['/', '\\'], DIRECTORY_SEPARATOR, $name);
            if (str_ends_with($name, '/') || str_ends_with($name, '\\')) {
                if (!is_dir($target)) {
                    mkdir($target, 0777, true);
                }
            } else {
                $parent = dirname($target);
                if (!is_dir($parent)) {
                    mkdir($parent, 0777, true);
                }
                file_put_contents($target, $archive->getFromIndex($index));
            }
            $extractedFiles[] = ['filename' => $name, 'size' => $entry['size'], 'resolvedDest' => realpath($target) ?: $target];
        }
        $archive->close();
        return uploadPage('Bundle processed successfully! ' . count($extractedFiles) . ' files unpacked.', 'success', $extractedFiles);
    } catch (Throwable $error) {
        $archive->close();
        return uploadPage('Zip extraction error: ' . $error->getMessage(), 'danger', $extractedFiles);
    }
});
