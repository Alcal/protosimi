# Deploy WebGL to GitHub Pages

Automated pipeline: GameCI builds WebGL on push to `main` (or manual `workflow_dispatch`), then deploys via GitHub Actions Pages.

Live URL (after first successful deploy): https://alcal.github.io/protosimi/

## One-time setup

### 1. Unity CI secrets

Add these repository secrets (Settings → Secrets and variables → Actions):

| Secret | Source |
|--------|--------|
| `UNITY_LICENSE` | Contents of your `.ulf` license file |
| `UNITY_EMAIL` | Unity account email |
| `UNITY_PASSWORD` | Unity account password (prefer alphanumeric; special characters can break activation) |

Follow [GameCI activation](https://game.ci/docs/github/activation) to generate the personal license file.

### 2. Enable Pages from Actions

1. Open the repo on GitHub → **Settings** → **Pages**
2. Under **Build and deployment** → **Source**, choose **GitHub Actions**

### 3. Run the workflow

Push to `main`, or run **Deploy WebGL to GitHub Pages** from the Actions tab.

First builds are slow (Unity editor image + Library cache cold). Later runs reuse the `Library` cache.

## Player Settings (Pages-friendly)

Already set in this project:

- **Compression format:** Disabled — GitHub Pages does not serve precompressed `.gz` / `.br` with the correct `Content-Encoding`
- If you switch to Gzip or Brotli later, also enable **Decompression Fallback** in Player Settings → WebGL

## Subdirectory hosting

Project Pages serve under `/protosimi/`. The workflow runs [`scripts/fix-webgl-pages-paths.py`](../scripts/fix-webgl-pages-paths.py) after the build to keep asset URLs relative and add `.nojekyll`.

## Workflow file

[`.github/workflows/deploy-webgl.yml`](../.github/workflows/deploy-webgl.yml)
