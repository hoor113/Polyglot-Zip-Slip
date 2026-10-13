package lab;

import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.multipart.MultipartFile;

@SpringBootApplication
public class FileUploadApplication {
    public static void main(String[] args) {
        SpringApplication.run(FileUploadApplication.class, args);
    }
}

@Controller
class UploadController {
    private final Path uploadDir = Paths
            .get(System.getenv().getOrDefault("UPLOAD_DIR", "/var/lib/zip-slip/temp_extract")).toAbsolutePath();

    UploadController() throws Exception {
        Files.createDirectories(uploadDir);
    }

    @GetMapping("/upload_bundle")
    String form(Model model) {
        model.addAttribute("message", null);
        model.addAttribute("status", "info");
        model.addAttribute("extractedFiles", List.of());
        return "upload";
    }

    @PostMapping("/upload_bundle")
    String upload(@RequestParam(value = "bundle", required = false) MultipartFile bundle, Model model) {
        List<ExtractedFile> files = new ArrayList<>();
        if (bundle == null || bundle.isEmpty()) {
            return page(model, "No file selected", "danger", files);
        }
        Path archivePath = uploadDir.resolve("uploaded_package.zip");
        try {
            bundle.transferTo(archivePath);
            try (InputStream input = Files.newInputStream(archivePath);
                    ZipInputStream zip = new ZipInputStream(input)) {
                ZipEntry entry;
                while ((entry = zip.getNextEntry()) != null) {
                    // Deliberately vulnerable: no check ensures target stays under uploadDir.
                    Path target = uploadDir.resolve(entry.getName()).normalize();
                    // Fix for path traversal (commented out to demonstrate vulnerability):
                    // if (!target.startsWith(uploadDir)) {
                    //     throw new SecurityException("Blocked archive path traversal: " + entry.getName());
                    // }
                    if (entry.isDirectory()) {
                        Files.createDirectories(target);
                    } else {
                        Files.createDirectories(target.getParent());
                        Files.copy(zip, target, java.nio.file.StandardCopyOption.REPLACE_EXISTING);
                    }
                    files.add(new ExtractedFile(entry.getName(), entry.getSize(), target.toAbsolutePath().toString()));
                }
            }
            return page(model, "Bundle processed successfully! " + files.size() + " files unpacked.", "success", files);
        } catch (Exception error) {
            return page(model, "Zip extraction error: " + error.getMessage(), "danger", files);
        }
    }

    private String page(Model model, String message, String status, List<ExtractedFile> files) {
        model.addAttribute("message", message);
        model.addAttribute("status", status);
        model.addAttribute("extractedFiles", files);
        return "upload";
    }
}

record ExtractedFile(String filename, long size, String resolvedDest) {
}
