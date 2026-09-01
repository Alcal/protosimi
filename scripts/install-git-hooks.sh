#!/usr/bin/env bash
# Install repo hooks into .git/hooks without changing git config.
set -euo pipefail
ROOT="$(git rev-parse --show-toplevel)"
HOOKS_DIR="$(git rev-parse --git-path hooks)"
mkdir -p "$HOOKS_DIR"
install -m 0755 "$ROOT/.githooks/post-commit" "$HOOKS_DIR/post-commit"
echo "Installed post-commit hook -> $HOOKS_DIR/post-commit"
