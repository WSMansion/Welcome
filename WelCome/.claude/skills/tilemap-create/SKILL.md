---
name: tilemap-create
description: Create a Grid GameObject with a child Tilemap + TilemapRenderer in the active scene. Optionally parent the Grid under an existing GameObject and name the tilemap. Returns the new tilemap GameObject reference and its instanceId.
metadata:
  generated-by: mcp-plugin-dotnet
---

# Tilemap / Create Grid + Tilemap

Create the standard 2D tilemap hierarchy: a `Grid` GameObject hosting a child GameObject with a `Tilemap` and `TilemapRenderer`. This is the minimal structure required before painting tiles.

## Inputs

- `gridName` — optional name for the Grid GameObject (default `Grid`).
- `tilemapName` — optional name for the Tilemap child GameObject (default `Tilemap`).
- `parentRef` — optional GameObject to parent the new Grid under.
- `cellSize` — optional Grid cell size (default `(1,1,0)`).

## Behavior

Creates the Grid + child Tilemap/TilemapRenderer, sets the cell size, parents under `parentRef` when supplied, marks the scene dirty, repaints, and returns references + the tilemap GameObject instanceId. Runs on the Unity main thread.

## How to Call

```bash
unity-mcp-cli run-tool tilemap-create --input '{
  "gridName": "string_value",
  "tilemapName": "string_value",
  "parentRef": "string_value",
  "cellSize": "string_value"
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use:
> ```bash
> unity-mcp-cli run-tool tilemap-create --input-file args.json
> ```
>
> Or pipe via stdin (recommended):
> ```bash
> unity-mcp-cli run-tool tilemap-create --input-file - <<'EOF'
> {"param": "value"}
> EOF
> ```


### Troubleshooting

If `unity-mcp-cli` is not found, either install it globally (`npm install -g unity-mcp-cli`) or use `npx unity-mcp-cli` instead.
Read the /unity-initial-setup skill for detailed installation instructions.

## Input

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `gridName` | `string` | No | Name of the Grid GameObject. |
| `tilemapName` | `string` | No | Name of the child Tilemap GameObject. |
| `parentRef` | `string` | No | Optional GameObject to parent the new Grid under. RQL: GameObjectRef |
| `cellSize` | `string` | No | Grid cell size. Defaults to (1,1,0). RQL: Vector3 |

### Input JSON Schema

```json
{
  "type": "object",
  "properties": {
    "gridName": {
      "type": "string"
    },
    "tilemapName": {
      "type": "string"
    },
    "parentRef": {
      "type": "string"
    },
    "cellSize": {
      "type": "string"
    }
  }
}
```

## Output

This tool does not return structured output.

