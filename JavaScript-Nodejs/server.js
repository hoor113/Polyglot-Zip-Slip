const express = require("express");
const multer = require("multer");
const AdmZip = require("adm-zip");
const fs = require("fs");
const path = require("path");

const app = express();
const port = Number(process.env.PORT || 3000);
const uploadDir = path.resolve(process.env.UPLOAD_DIR || "/var/lib/zip-slip/temp_extract");
fs.mkdirSync(uploadDir, { recursive: true });

const upload = multer({ dest: uploadDir });
app.set("view engine", "ejs");
app.set("views", path.join(__dirname, "views"));
app.use("/static", express.static(path.join(__dirname, "static")));

app.get("/upload_bundle", (_req, res) => {
  res.render("upload", { message: null, status: "info", extractedFiles: [] });
});

app.post("/upload_bundle", upload.single("bundle"), (req, res) => {
  if (!req.file) {
    return res.status(400).render("upload", {
      message: "No file selected",
      status: "danger",
      extractedFiles: []
    });
  }

  const archivePath = path.join(uploadDir, "uploaded_package.zip");
  fs.copyFileSync(req.file.path, archivePath);
  const extractedFiles = [];

  try {
    const archive = new AdmZip(archivePath);
    for (const entry of archive.getEntries()) {
      // Deliberately vulnerable: the archive entry is not constrained to uploadDir.
      const target = path.normalize(path.join(uploadDir, entry.entryName));
      // Fix for Zip Slip
      // if (!target.startsWith(uploadDir)) {
      //   throw new Error('Directory traversal detected');
      // }
      fs.mkdirSync(path.dirname(target), { recursive: true });
      fs.writeFileSync(target, entry.getData());
      extractedFiles.push({
        filename: entry.entryName,
        size: entry.header.size,
        resolvedDest: path.resolve(target)
      });
    }
    return res.render("upload", {
      message: `Bundle processed successfully! ${extractedFiles.length} files unpacked.`,
      status: "success",
      extractedFiles
    });
  } catch (error) {
    return res.status(500).render("upload", {
      message: `Zip extraction error: ${error.message}`,
      status: "danger",
      extractedFiles
    });
  } finally {
    fs.rmSync(req.file.path, { force: true });
  }
});

app.listen(port, "0.0.0.0", () => {
  console.log(`Node.js Zip Slip lab listening on ${port}`);
});
