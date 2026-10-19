---
name: tilemap-list
description: List every Tilemap in the active scene with its name, painted-tile count, cell bounds, and orientation. Read-only.
metadata:
  generated-by: mcp-plugin-dotnet
---

# Tilemap / List Tilemaps

Enumerate all `Tilemap` components in the active scene.

## Inputs

- `includeInactive` (bool, default true) — include Tilemaps on inactive GameObjects.

## Behavior

Finds all `Tilemap` instances, reads each one's `GetUsedTilesCount`, `cellBounds`, and `orientation`, and returns them. Read-only. Runs on the Unity main thread.

## How to Call

```bash
unity-mcp-cli run-tool tilemap-list --input '{
  "includeInactive": false
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use:
> ```bash
> unity-mcp-cli run-tool tilemap-list --input-file args.json
> ```
>
> Or pipe via stdin (recommended):
> ```bash
> unity-mcp-cli run-tool tilemap-list --input-file - <<'EOF'
> {"param": "value"}
> EOF
> ```


### Troubleshooting

If `unity-mcp-cli` is not found, either install it globally (`npm install -g unity-mcp-cli`) or use `npx unity-mcp-cli` instead.
Read the /unity-initial-setup skill for detailed installation instructions.

## Input

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `includeInactive` | `boolean` | No | If true (default), include Tilemaps on inactive GameObjects. |

### Input JSON Schema

```json
{
  "type": "object",
  "properties": {
    "includeInactive": {
      "type": "boolean"
    }
  }
}
```

## Output

This tool does not return structured output.

