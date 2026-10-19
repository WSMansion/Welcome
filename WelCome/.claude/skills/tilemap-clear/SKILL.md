---
name: tilemap-clear
description: "Clear tiles from a Tilemap: either every tile (ClearAllTiles) or a rectangular region (by erasing each cell in the inclusive min..max range). Destructive."
metadata:
  generated-by: mcp-plugin-dotnet
---

# Tilemap / Clear Tiles

Erase tiles from a `Tilemap`. By default clears the whole map; pass a region to clear only a rectangular block.

## Inputs

- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).
- `clearAll` (bool, default true) — when true, calls `Tilemap.ClearAllTiles()`; when false, erases the region defined by `minX/minY/maxX/maxY`.
- `minX`, `minY`, `maxX`, `maxY`, `z` — the inclusive region (used only when `clearAll` is false).

## Behavior

Either clears all tiles or sets each cell in the region to null, marks the scene dirty, and repaints. Runs on the Unity main thread.

## How to Call

```bash
unity-mcp-cli run-tool tilemap-clear --input '{
  "gameObjectRef": "string_value",
  "clearAll": false,
  "minX": 0,
  "minY": 0,
  "maxX": 0,
  "maxY": 0,
  "z": 0
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use:
> ```bash
> unity-mcp-cli run-tool tilemap-clear --input-file args.json
> ```
>
> Or pipe via stdin (recommended):
> ```bash
> unity-mcp-cli run-tool tilemap-clear --input-file - <<'EOF'
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
| `clearAll` | `boolean` | No | If true (default) clear the entire Tilemap; if false, clear the min..max region only. |
| `minX` | `integer` | No | Inclusive minimum cell X (region mode). |
| `minY` | `integer` | No | Inclusive minimum cell Y (region mode). |
| `maxX` | `integer` | No | Inclusive maximum cell X (region mode). |
| `maxY` | `integer` | No | Inclusive maximum cell Y (region mode). |
| `z` | `integer` | No | Cell Z coordinate (region mode, default 0). |

### Input JSON Schema

```json
{
  "type": "object",
  "properties": {
    "gameObjectRef": {
      "type": "string"
    },
    "clearAll": {
      "type": "boolean"
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
    "gameObjectRef"
  ]
}
```

## Output

This tool does not return structured output.

