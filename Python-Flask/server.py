import os
import shutil
import zipfile
from flask import Flask, flash, redirect, render_template, request

app = Flask(__name__)
app.secret_key = os.environ.get("SECRET_KEY", "zip-slip-lab")
UPLOAD_DIR = os.path.abspath(os.environ.get("UPLOAD_DIR", "/var/lib/zip-slip/temp_extract"))
os.makedirs(UPLOAD_DIR, exist_ok=True)

@app.route("/upload_bundle", methods=["GET", "POST"])
def upload_bundle():
    message = None
    extracted_files = []
    status = "info"
    if request.method == "POST":
        if "bundle" not in request.files:
            flash("No file part in the request", "danger")
            return redirect(request.url)
        bundle = request.files["bundle"]
        if not bundle.filename:
            flash("No file selected", "danger")
            return redirect(request.url)
        if not bundle.filename.lower().endswith(".zip"):
            flash("Only .zip bundles are accepted", "danger")
            return redirect(request.url)
        archive_path = os.path.join(UPLOAD_DIR, "uploaded_package.zip")
        bundle.save(archive_path)
        try:
            with zipfile.ZipFile(archive_path) as archive:
                for member in archive.infolist():
                    target = os.path.normpath(os.path.join(UPLOAD_DIR, member.filename.replace("/", os.sep)))
                    os.makedirs(os.path.dirname(target), exist_ok=True)
                    with archive.open(member) as source, open(target, "wb") as destination:
                        shutil.copyfileobj(source, destination)
                    extracted_files.append({"filename": member.filename, "size": member.file_size, "resolved_dest": os.path.abspath(target)})
            message = f"Bundle processed successfully! {len(extracted_files)} files unpacked."
            status = "success"
        except Exception as exc:
            message = f"Zip extraction error: {exc}"
            status = "danger"
    return render_template("upload.html", message=message, extracted_files=extracted_files, status=status)

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=int(os.environ.get("PORT", 80)), debug=False)
