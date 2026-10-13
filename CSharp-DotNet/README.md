# File Upload / Zip Slip - C# ASP.NET Core

Minimal ASP.NET Core implementation with the same `/upload_bundle` GET and POST contract as the other labs. ZIP entries are intentionally written without a canonical destination check.

Run with `dotnet run --urls http://localhost:8080`, or build the included Dockerfile.

## Build a proof-of-concept payload

The `PayloadBuilder` console project creates a harmless archive containing a
path-traversal entry. Its default entry writes `zip-slip-dotnet-proof.txt` one
directory above `/var/lib/zip-slip/temp_extract`, keeping the demonstration
inside the lab container:

```powershell
dotnet run --project PayloadBuilder
```

Upload the generated `exploit.zip` at `http://localhost:8080/upload_bundle`.
Customize the archive entry or marker when testing another isolated lab target:

```powershell
dotnet run --project PayloadBuilder -- `
  --output .\artifacts\dotnet-zipslip.zip `
  --entry-name ..\zip-slip-dotnet-proof.txt `
  --marker ZIP_SLIP_DOTNET_PROOF_OF_CONCEPT
```

Only use this payload against the intentionally vulnerable lab or another
system you are authorized to test.
