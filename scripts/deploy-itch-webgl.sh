#!/usr/bin/env bash
# Build WebGL locally and push to itch.io (acidosys/manos-limpias:html).
# Skip with SKIP_ITCH_DEPLOY=1. Requires `unity` and `butler` on PATH.
set -euo pipefail

ITCH_CHANNEL="${ITCH_CHANNEL:-acidosys/manos-limpias:html}"
UNITY_TIMEOUT="${UNITY_BUILD_TIMEOUT:-3600}"

ROOT="$(git rev-parse --show-toplevel)"
cd "$ROOT"

if ! command -v unity >/dev/null 2>&1; then
  echo "deploy-itch-webgl: unity CLI not found on PATH" >&2
  exit 1
fi
if ! command -v butler >/dev/null 2>&1; then
  echo "deploy-itch-webgl: butler not found on PATH (https://itch.io/docs/butler/)" >&2
  exit 1
fi

if [[ -f "$ROOT/Library/EditorInstance.json" ]]; then
  echo "deploy-itch-webgl: Unity Editor has this project open. Close it, then retry:" >&2
  echo "  $ROOT/scripts/deploy-itch-webgl.sh" >&2
  exit 1
fi

stashed=0
if [[ -n "$(git status --porcelain)" ]]; then
  git stash push --include-untracked --quiet -m "itch-deploy auto-stash"
  stashed=1
  trap 'if [[ "$stashed" -eq 1 ]]; then git stash pop --quiet || true; fi' EXIT
fi

echo "deploy-itch-webgl: building WebGL (this can take a while)..."
unity build "$ROOT" \
  --target WebGL \
  --execute-method ManosLimpias.Editor.WebGLItchBuild.PerformBuild \
  --output-path "$ROOT/build/WebGL" \
  --allow-dirty-build \
  --timeout "$UNITY_TIMEOUT"

INDEX="$(find "$ROOT/build" -type f -name index.html | head -n 1)"
if [[ -z "$INDEX" ]]; then
  echo "deploy-itch-webgl: no index.html under build/" >&2
  exit 1
fi
OUT_DIR="$(dirname "$INDEX")"
VERSION="$(git rev-parse --short HEAD)"

echo "deploy-itch-webgl: butler push $OUT_DIR -> $ITCH_CHANNEL ($VERSION)"
butler push "$OUT_DIR" "$ITCH_CHANNEL" --userversion "$VERSION"
echo "deploy-itch-webgl: done → https://acidosys.itch.io/manos-limpias"
