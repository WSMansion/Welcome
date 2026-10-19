---
name: tilemap-get-tile
description: "Read the tile occupying a single cell of a Tilemap: returns whether the cell has a tile, the tile asset name + path, the tile color, and the collider type at that cell. Read-only."
metadata:
  generated-by: mcp-plugin-dotnet
---

# Tilemap / Get Tile

Inspect a single cell of a `Tilemap`.

## Inputs

- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).
- `x`, `y`, `z` — the integer cell coordinate (z defaults to 0).

## Behavior

Reads `Tilemap.GetTile`, `GetColor`, and `GetColliderType` at the cell and returns them. When the cell is empty, `hasTile` is false. Read-only. Runs on the Unity main thread.

## How to Call

```bash
unity-mcp-cli run-tool tilemap-get-tile --input '{
  "gameObjectRef": "string_value",
  "x": 0,
  "y": 0,
  "z": 0
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use:
> ```bash
> unity-mcp-cli run-tool tilemap-get-tile --input-file args.json
> ```
>
> Or pipe via stdin (recommended):
> ```bash
> unity-mcp-cli run-tool tilemap-get-tile --input-file - <<'EOF'
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

