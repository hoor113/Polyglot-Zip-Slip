using System.IO.Compression;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var uploadDir = Path.GetFullPath(Environment.GetEnvironmentVariable("UPLOAD_DIR") ?? "/var/lib/zip-slip/temp_extract");
Directory.CreateDirectory(uploadDir);

app.MapGet("/upload_bundle", () => Results.Content(RenderPage(null, "info", []), "text/html"));
app.MapPost("/upload_bundle", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var bundle = form.Files.GetFile("bundle");
    if (bundle is null || bundle.Length == 0)
        return Results.Content(RenderPage("No file selected", "danger", []), "text/html", Encoding.UTF8, 400);

    var archivePath = Path.Combine(uploadDir, "uploaded_package.zip");
    await using (var output = File.Create(archivePath))
        await bundle.CopyToAsync(output);

    var extracted = new List<ExtractedFile>();
    try
    {
        using var archive = ZipFile.OpenRead(archivePath);
        foreach (var entry in archive.Entries)
        {
            var fullUploadDir = Path.GetFullPath(uploadDir);
            // Deliberately vulnerable: no canonical destination-prefix validation.
            var target = Path.GetFullPath(Path.Combine(uploadDir, entry.FullName.Replace('/', Path.DirectorySeparatorChar)));
            // Following fix for Zip Slip
            // if (!target.StartsWith(fullUploadDir, StringComparison.OrdinalIgnoreCase))
            //     throw new InvalidOperationException("Directory traversal detected.");
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            await using var source = entry.Open();
            await using var destination = File.Create(target);
            await source.CopyToAsync(destination);
            extracted.Add(new ExtractedFile(entry.FullName, entry.Length, target));
        }
        return Results.Content(RenderPage($"Bundle processed successfully! {extracted.Count} files unpacked.", "success", extracted), "text/html");
    }
    catch (Exception error)
    {
        return Results.Content(RenderPage($"Zip extraction error: {WebUtility.HtmlEncode(error.Message)}", "danger", extracted), "text/html", Encoding.UTF8, 500);
    }
});

app.UseStaticFiles();
app.Run();

static string RenderPage(string? message, string status, IReadOnlyList<ExtractedFile> files)
{
    var rows = string.Join("", files.Select(file =>
        $"<tr><td><code class=\"badge badge-warning\">{WebUtility.HtmlEncode(file.Name)}</code></td><td>{file.Size} B</td><td><code>{WebUtility.HtmlEncode(file.Destination)}</code></td></tr>"));
    var feedback = message is null ? "" : $"<div class=\"alert alert-{status}\">{WebUtility.HtmlEncode(message)}</div>";
    var table = files.Count == 0
        ? "<div class=\"empty\">No bundle processed yet. Upload a valid <code>.zip</code> file to inspect extraction output.</div>"
        : $"<table class=\"data-table\"><thead><tr><th>Archive Path</th><th>Size</th><th>Target Destination</th></tr></thead><tbody>{rows}</tbody></table>";
    return $$"""
    <!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
    <title>Audit Bundle Upload - Zip Slip Exploration</title><link rel="stylesheet" href="/style.css"></head>
    <body><header class="navbar"><div class="container nav-container"><a class="brand" href="/upload_bundle"><span class="brand-badge">ZIP</span><span>Archive Upload Lab</span></a><a class="nav-link" href="/upload_bundle">Upload Bundle</a></div></header>
    <main class="main-content"><div class="container"><div class="page-header"><h1 class="page-title">Audit Bundle Ingestion Center</h1><p class="page-subtitle">Upload zipped bundles containing audit assets, template fragments, and reporting logs.</p></div>
    <div class="grid"><div class="card"><h3 class="card-title">📦 Upload Audit Archive (.zip)</h3><form method="POST" action="/upload_bundle" enctype="multipart/form-data"><div class="form-group"><label class="form-label">Select ZIP Bundle:</label><input type="file" name="bundle" accept=".zip" class="form-input" required></div><button class="btn btn-primary" type="submit">🚀 Upload &amp; Unpack Bundle</button></form><div class="note"><h4>💡 Zip Slip Architecture Note:</h4><p>The extraction directory is <code>/var/lib/zip-slip/temp_extract/</code>. Archive entries are not canonicalized before writing.</p></div></div>
    <div class="card"><h3 class="card-title">📋 Extraction Log &amp; Status</h3>{{feedback}}{{table}}</div></div></div></main></body></html>
    """;
}

record ExtractedFile(string Name, long Size, string Destination);
