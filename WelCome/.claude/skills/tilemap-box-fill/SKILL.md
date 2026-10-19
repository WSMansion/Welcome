---
name: tilemap-box-fill
description: Fill a rectangular region of a Tilemap with a single TileBase asset using Tilemap.BoxFill. The region is defined by an inclusive min and max cell coordinate.
metadata:
  generated-by: mcp-plugin-dotnet
---

# Tilemap / Box Fill

Fill a rectangular block of cells of a `Tilemap` with one tile via `Tilemap.BoxFill`.

## Inputs

- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).
- `minX`, `minY` — inclusive minimum cell coordinate.
- `maxX`, `maxY` — inclusive maximum cell coordinate.
- `z` — cell Z coordinate (default 0).
- `tileAssetPath` — `Assets/`-rooted path to the `TileBase` asset to fill with (required).

## Behavior

Loads the tile, calls `Tilemap.BoxFill(start, tile, minX, minY, maxX, maxY)`, marks the scene dirty, and repaints. Returns the filled cell count. Runs on the Unity main thread.

## How to Call

```bash
unity-mcp-cli run-tool tilemap-box-fill --input '{
  "gameObjectRef": "string_value",
  "tileAssetPath": "string_value",
  "minX": 0,
  "minY": 0,
  "maxX": 0,
  "maxY": 0,
  "z": 0
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use:
> ```bash
> unity-mcp-cli run-tool tilemap-box-fill --input-file args.json
> ```
>
> Or pipe via stdin (recommended):
> ```bash
> unity-mcp-cli run-tool tilemap-box-fill --input-file - <<'EOF'
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
| `tileAssetPath` | `string` | Yes | Assets/-rooted path to the TileBase asset to fill the region with. |
| `minX` | `integer` | Yes | Inclusive minimum cell X coordinate. |
| `minY` | `integer` | Yes | Inclusive minimum cell Y coordinate. |
| `maxX` | `integer` | Yes | Inclusive maximum cell X coordinate. |
| `maxY` | `integer` | Yes | Inclusive maximum cell Y coordinate. |
| `z` | `integer` | No | Cell Z coordinate (default 0). |

### Input JSON Schema

```json
{
  "type": "object",
  "properties": {
    "gameObjectRef": {
      "type": "string"
    },
    "tileAssetPath": {
      "type": "string"
    },
    "minX": {
      "type": "integer"
    },
    "minY": {
      "type": "integer"
    },
    "maxX": {
      "type": "integer"
    },
    "maxY": {
      "type": "integer"
    },
    "z": {
      "type": "integer"
    }
  },
  "required": [
    "gameObjectRef",
    "tileAssetPath",
    "minX",
    "minY",
    "maxX",
    "maxY"
  ]
}
```

## Output

This tool does not return structured output.

