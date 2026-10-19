---
name: tilemap-set-tile-flags
description: Set the per-cell tint color and/or transform (flip X/Y, Z rotation, scale) of a tile already painted into a Tilemap. Unlocks the relevant TileFlags so the overrides take effect.
metadata:
  generated-by: mcp-plugin-dotnet
---

# Tilemap / Set Tile Color + Transform

Override the per-cell color and transform of a single tile in a `Tilemap`.

## Inputs

- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).
- `x`, `y`, `z` — the cell coordinate (z defaults to 0).
- `color` — optional per-cell tint color; unlocks `TileFlags.LockColor` when applied.
- `flipX`, `flipY` — optional booleans to mirror the tile.
- `rotationZ` — optional Z rotation in degrees.
- `scale` — optional uniform/again per-axis scale (default identity).

## Behavior

When a color is given, removes `TileFlags.LockColor` then calls `SetColor`. When any transform argument is given, removes `TileFlags.LockTransform` then builds a TRS matrix and calls `SetTransformMatrix`. Marks the scene dirty and repaints. Runs on the Unity main thread.

## How to Call

```bash
unity-mcp-cli run-tool tilemap-set-tile-flags --input '{
  "gameObjectRef": "string_value",
  "x": 0,
  "y": 0,
  "z": 0,
  "color": "string_value",
  "flipX": false,
  "flipY": false,
  "rotationZ": 0,
  "scale": "string_value"
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use:
> ```bash
> unity-mcp-cli run-tool tilemap-set-tile-flags --input-file args.json
> ```
>
> Or pipe via stdin (recommended):
> ```bash
> unity-mcp-cli run-tool tilemap-set-tile-flags --input-file - <<'EOF'
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
| `color` | `string` | No | Optional per-cell tint color. RQL: Color |
| `flipX` | `boolean` | No | Optional: mirror the tile horizontally. |
| `flipY` | `boolean` | No | Optional: mirror the tile vertically. |
| `rotationZ` | `number` | No | Optional: Z rotation in degrees. |
| `scale` | `string` | No | Optional per-axis scale (default (1,1,1)). RQL: Vector3 |

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
    "color": {
      "type": "string"
    },
    "flipX": {
      "type": "boolean"
    },
    "flipY": {
      "type": "boolean"
    },
    "rotationZ": {
      "type": "number"
    },
    "scale": {
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

