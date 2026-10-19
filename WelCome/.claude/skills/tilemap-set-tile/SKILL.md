---
name: tilemap-set-tile
description: Paint a single tile (a TileBase asset such as a Tile or RuleTile) into a Tilemap at a given cell coordinate. Pass a null/empty tileAssetPath to erase the cell.
metadata:
  generated-by: mcp-plugin-dotnet
---

# Tilemap / Set Tile

Set the tile at a single cell of a `Tilemap`. The tile is loaded from an `Assets/`-rooted path to any `TileBase` asset (a `Tile`, `RuleTile`, etc.).

## Inputs

- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).
- `x`, `y`, `z` — the integer cell coordinate (z defaults to 0).
- `tileAssetPath` — `Assets/`-rooted path to the `TileBase` asset; null/empty erases the cell.

## Behavior

Loads the tile asset (when supplied), calls `Tilemap.SetTile(cell, tile)`, marks the scene dirty, and repaints. Runs on the Unity main thread.

## How to Call

```bash
unity-mcp-cli run-tool tilemap-set-tile --input '{
  "gameObjectRef": "string_value",
  "x": 0,
  "y": 0,
  "z": 0,
  "tileAssetPath": "string_value"
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use:
> ```bash
> unity-mcp-cli run-tool tilemap-set-tile --input-file args.json
> ```
>
> Or pipe via stdin (recommended):
> ```bash
> unity-mcp-cli run-tool tilemap-set-tile --input-file - <<'EOF'
> {"param": "value"}
> EOF
> ```


### Troubleshooting

If `unity-mcp-cli` is not found, either install it globally (`npm install -g unity-mcp-cli`) or use `npx unity-mcp-cli` instead.
Read the /unity-initial-setup skill for detailed installation instructions.

## Input

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `gameObjectRef` | `string` | Yes | Reference to the GameObject containing the Tilemap component. RQL: GameObjectRef |
| `x` | `integer` | Yes | Cell X coordinate. |
| `y` | `integer` | Yes | Cell Y coordinate. |
| `z` | `integer` | No | Cell Z coordinate (default 0). |
| `tileAssetPath` | `string` | No | Assets/-rooted path to the TileBase asset to paint. Null/empty erases the cell. |

### Input JSON Schema

```json
{
  "type": "object",
  "properties": {
    "gameObjectRef": {
      "type": "string"
    },
    "x": {
      "type": "integer"
    },
    "y": {
      "type": "integer"
    },
    "z": {
      "type": "integer"
    },
    "tileAssetPath": {
      "type": "string"
    }
  },
  "required": [
    "gameObjectRef",
    "x",
    "y"
  ]
}
```

## Output

This tool does not return structured output.

