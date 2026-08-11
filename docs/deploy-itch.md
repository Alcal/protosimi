# Deploy WebGL to itch.io

Automated pipeline: GameCI builds WebGL on push to `main` (or manual `workflow_dispatch`), then uploads with [Butler](https://itch.io/docs/butler/) to itch.io.

Target: `acidosys/manos-limpias:html`  
Play URL: https://acidosys.itch.io/manos-limpias

## Secrets

| Secret | Source |
|--------|--------|
| `UNITY_LICENSE` | Contents of your `.ulf` license file |
| `UNITY_EMAIL` | Unity account email |
| `UNITY_PASSWORD` | Unity account password (prefer alphanumeric) |
| `BUTLER_API_KEY` | itch.io API key ([account settings](https://itch.io/user/settings/api-keys)) |

Unity license setup: [GameCI activation](https://game.ci/docs/github/activation).

## Run

Push to `main`, or run **Deploy WebGL to itch.io** from the Actions tab.

First builds are slow (Unity editor image + Library cache cold). Later runs reuse the `Library` cache.

## Player Settings

- **Compression format:** Disabled — safest for browser hosting embeds (itch HTML channel)

## Workflow file

[`.github/workflows/deploy-itch.yml`](../.github/workflows/deploy-itch.yml)

GitHub Pages deploy is disabled for now (see git history for the previous Pages workflow).
