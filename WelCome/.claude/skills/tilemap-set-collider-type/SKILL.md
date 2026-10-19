---
name: tilemap-set-collider-type
description: Set the per-cell Tile.ColliderType (None / Sprite / Grid) of a tile painted into a Tilemap. Pair with a TilemapCollider2D to generate physics colliders from the tiles.
metadata:
  generated-by: mcp-plugin-dotnet
---

# Tilemap / Set Tile Collider Type

Override the collider type of a single tile cell in a `Tilemap`.

## Inputs

- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).
- `x`, `y`, `z` — the cell coordinate (z defaults to 0).
- `colliderType` — `None`, `Sprite`, or `Grid`.

## Behavior

Maps the argument to `UnityEngine.Tilemaps.Tile.ColliderType` and calls `Tilemap.SetColliderType`, marks the scene dirty, and repaints. Runs on the Unity main thread.

## How to Call

```bash
unity-mcp-cli run-tool tilemap-set-collider-type --input '{
  "gameObjectRef": "string_value",
  "x": 0,
  "y": 0,
  "colliderType": "string_value",
  "z": 0
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use:
> ```bash
> unity-mcp-cli run-tool tilemap-set-collider-type --input-file args.json
> ```
>
> Or pipe via stdin (recommended):
> ```bash
> unity-mcp-cli run-tool tilemap-set-collider-type --input-file - <<'EOF'
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
| `colliderType` | `string` | Yes | The collider type to set: None, Sprite, or Grid. |
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
    "colliderType": {
      "type": "string",
      "enum": [
        "None",
        "Sprite",
        "Grid"
      ]
    },
    "z": {
      "type": "integer"
    }
  },
  "required": [
    "gameObjectRef",
    "x",
    "y",
    "colliderType"
  ]
}
```

## Output

This tool does not return structured output.

