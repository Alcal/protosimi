#!/usr/bin/env python3
"""Rewrite Unity WebGL index.html for GitHub project Pages (/repo-name/)."""

from __future__ import annotations

import pathlib
import re
import sys


def main() -> int:
    if len(sys.argv) != 2:
        print(f"Usage: {sys.argv[0]} <webgl-output-dir>", file=sys.stderr)
        return 2

    out = pathlib.Path(sys.argv[1])
    index = out / "index.html"
    if not index.is_file():
        print(f"Missing {index}", file=sys.stderr)
        return 1

    text = index.read_text(encoding="utf-8")
    original = text

    # Absolute "/Build" or "/Build/…" breaks under https://user.github.io/repo/
    text = re.sub(r'(["\'])/(Build)\b', r"\1./\2", text)
    text = re.sub(r'(url:\s*)(["\'])/', r"\1\2./", text)

    if "<base " not in text.lower():
        text = text.replace("<head>", '<head>\n    <base href="./">', 1)

    if text != original:
        index.write_text(text, encoding="utf-8")
        print("Rewrote index.html for subdirectory hosting")
    else:
        print("index.html already uses relative paths")

    (out / ".nojekyll").write_text("", encoding="utf-8")
    print(f"Wrote {out / '.nojekyll'}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
