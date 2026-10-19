---
name: tilemap-create-tile-asset
description: Create a UnityEngine.Tilemaps.Tile asset at an Assets/-rooted path and assign a Sprite (loaded from a sprite asset path) plus an optional color and collider type. Returns the created asset path.
metadata:
  generated-by: mcp-plugin-dotnet
---

# Tilemap / Create Tile Asset

Create a `Tile` ScriptableObject asset and wire its sprite so it can be painted into a Tilemap.

## Inputs

- `assetPath` — `Assets/`-rooted path ending in `.asset` for the new Tile.
- `spriteAssetPath` — optional `Assets/`-rooted path to a `Sprite` asset to assign.
- `color` — optional tile color (default white).
- `colliderType` — optional `None` / `Sprite` / `Grid` (default Sprite).

## Behavior

Creates the intermediate folders, instantiates a `Tile`, assigns sprite/color/collider type, writes the asset via `AssetDatabase.CreateAsset`, saves + refreshes, and returns the asset path. Runs on the Unity main thread.

## How to Call

```bash
unity-mcp-cli run-tool tilemap-create-tile-asset --input '{
  "assetPath": "string_value",
  "spriteAssetPath": "string_value",
  "color": "string_value",
  "colliderType": "string_value"
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use:
> ```bash
> unity-mcp-cli run-tool tilemap-create-tile-asset --input-file args.json
> ```
>
> Or pipe via stdin (recommended):
> ```bash
> unity-mcp-cli run-tool tilemap-create-tile-asset --input-file - <<'EOF'
> {"param": "value"}
> EOF
> ```


### Troubleshooting

If `unity-mcp-cli` is not found, either install it globally (`npm install -g unity-mcp-cli`) or use `npx unity-mcp-cli` instead.
Read the /unity-initial-setup skill for detailed installation instructions.

## Input

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `assetPath` | `string` | Yes | Assets/-rooted path ending in '.asset' for the new Tile asset. |
| `spriteAssetPath` | `string` | No | Optional Assets/-rooted path to a Sprite asset to assign to the tile. |
| `color` | `string` | No | Optional tile color (default white). RQL: Color |
| `colliderType` | `string` | No | Optional collider type (default Sprite). |

### Input JSON Schema

```json
{
  "type": "object",
  "properties": {
    "assetPath": {
      "type": "string"
    },
    "spriteAssetPath": {
      "type": "string"
    },
    "color": {
      "type": "string"
    },
    "colliderType": {
      "type": "string",
      "enum": [
        "None",
        "Sprite",
        "Grid"
      ]
    }
  },
  "required": [
    "assetPath"
  ]
}
```

## Output

This tool does not return structured output.

