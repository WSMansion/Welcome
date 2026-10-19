---
name: tilemap-set-orientation
description: Set the tile anchor (sub-cell pivot, default (0.5,0.5,0)) and/or the layout orientation (XY/XZ/YX/YZ/ZX/ZY/Custom) of a Tilemap.
metadata:
  generated-by: mcp-plugin-dotnet
---

# Tilemap / Set Anchor + Orientation

Configure how a `Tilemap` positions and orients its tiles.

## Inputs

- `gameObjectRef` — the GameObject hosting the `Tilemap` (required).
- `anchor` — optional sub-cell anchor (tile pivot within a cell), default `(0.5,0.5,0)`.
- `orientation` — optional layout plane: `XY` (default 2D), `XZ`, `YX`, `YZ`, `ZX`, `ZY`, `Custom`.

## Behavior

Sets `Tilemap.tileAnchor` and/or `Tilemap.orientation`, marks the scene dirty, and repaints. Runs on the Unity main thread.

## How to Call

```bash
unity-mcp-cli run-tool tilemap-set-orientation --input '{
  "gameObjectRef": "string_value",
  "anchor": "string_value",
  "orientation": "string_value"
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use:
> ```bash
> unity-mcp-cli run-tool tilemap-set-orientation --input-file args.json
> ```
>
> Or pipe via stdin (recommended):
> ```bash
> unity-mcp-cli run-tool tilemap-set-orientation --input-file - <<'EOF'
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
| `anchor` | `string` | No | Optional sub-cell tile anchor (default (0.5,0.5,0)). RQL: Vector3 |
| `orientation` | `string` | No | Optional layout orientation plane. |

### Input JSON Schema

```json
{
  "type": "object",
  "properties": {
    "gameObjectRef": {
      "type": "string"
    },
    "anchor": {
      "type": "string"
    },
    "orientation": {
      "type": "string",
      "enum": [
        "XY",
        "XZ",
        "YX",
        "YZ",
        "ZX",
        "ZY",
        "Custom"
      ]
    }
  },
  "required": [
    "gameObjectRef"
  ]
}
```

## Output

This tool does not return structured output.

