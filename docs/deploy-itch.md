# Deploy WebGL to itch.io

A **post-commit hook** on `main` builds WebGL locally and uploads with [Butler](https://itch.io/docs/butler/) to itch.io.

Target: `acidosys/manos-limpias:html`  
Play URL: https://acidosys.itch.io/manos-limpias

## Install the hook (once)

```bash
./scripts/install-git-hooks.sh
```

That copies `.githooks/post-commit` into `.git/hooks/post-commit`. Git hooks are not versioned inside `.git/`, so each clone needs this once.

## What happens

Committing **on `main`** runs `scripts/deploy-itch-webgl.sh`:

1. `unity build --target WebGL` (Unity 6000.4.2f1)
2. `butler push` to `acidosys/manos-limpias:html`

The Unity Editor must **not** have this project open (it holds the project lock). Close the Editor, then commit, or run the script after closing.

## Skip or run by hand

```bash
SKIP_ITCH_DEPLOY=1 git commit -m "..."   # commit on main without deploying
./scripts/deploy-itch-webgl.sh             # deploy current tree without a commit
```

Requires `unity` and `butler` on PATH. Butler uses your existing `butler login` credentials (no GitHub secrets).

## Player Settings

- **Compression format:** Disabled — safest for browser hosting embeds (itch HTML channel)
