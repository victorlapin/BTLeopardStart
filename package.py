#!/usr/bin/env python3
"""Builds the release zip for BTLeopardStart.

The mod is modpack-agnostic — a single self-contained drop-in zip:
mod.json + settings.json + the DLL + README/LICENSE.

Usage:
    python package.py

Output: dist/BTLeopardStart-v<version>.zip (version from mod.json; a mismatch
between mod.json and the AssemblyVersion in src/Core.cs is an error).
"""

import json
import pathlib
import re
import subprocess
import sys
import zipfile

ROOT = pathlib.Path(__file__).resolve().parent
DIST = ROOT / "dist"
DLL = ROOT / "bin" / "Release" / "net472" / "BTLeopardStart.dll"
MOD = "BTLeopardStart"


def assembly_version() -> str:
    core = (ROOT / "src" / "Core.cs").read_text(encoding="utf-8-sig")
    m = re.search(r'AssemblyVersion\("([^"]+)"\)', core)
    if not m:
        raise SystemExit("cannot read AssemblyVersion from src/Core.cs")
    return m.group(1)


def main() -> None:
    print("building DLL (Release)...")
    subprocess.run(["dotnet", "build", str(ROOT / "BTLeopardStart.csproj"), "-c", "Release"],
                   cwd=ROOT, check=True)
    if not DLL.exists():
        raise SystemExit(f"DLL not found after build: {DLL}")

    version = assembly_version()
    mod_json = json.loads((ROOT / "mod.json").read_text(encoding="utf-8-sig"))
    mod_version = mod_json.get("Version")
    if mod_version != version:
        raise SystemExit(
            f"version mismatch: mod.json={mod_version}, AssemblyVersion={version} — bump both"
        )

    DIST.mkdir(exist_ok=True)
    zip_path = DIST / f"{MOD}-v{version}.zip"
    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
        # the zip contains the mod folder itself, so it extracts into Mods/
        zf.write(ROOT / "mod.json", f"{MOD}/mod.json")
        zf.write(DLL, f"{MOD}/{MOD}.dll")
        zf.write(ROOT / "settings.json", f"{MOD}/settings.json")
        zf.write(ROOT / "README.md", f"{MOD}/README.md")
        zf.write(ROOT / "LICENSE", f"{MOD}/LICENSE")
    print(f"packaged: {zip_path.name}")

    with zipfile.ZipFile(zip_path) as zf:
        for name in zf.namelist():
            print(f"  {name} ({zf.getinfo(name).file_size} bytes)")


if __name__ == "__main__":
    main()
