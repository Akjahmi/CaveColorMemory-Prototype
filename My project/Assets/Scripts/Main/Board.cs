using System.Collections.Generic;
using UnityEngine;
using HTW.CAVE;

// The floor: creates the tiles, shuffles and shows their colours,
// and converts a position in the room into a tile.
// A tile is addressed as Vector2Int(row, column).
public class Board : MonoBehaviour
{
    public int gridSize = 4;
    public float tileSize = 1f;
    public float gap = 0.08f;

    // How far (m) a player must step past a tile border before the tile changes.
    // Stops flickering when someone stands on a border.
    public float hysteresis = 0.08f;

    // Leave empty: the CAVE is found automatically.
    public VirtualEnvironment environment;

    private Renderer[,] tileRenderers;
    private int[,] colorIndices;

    public int TileCount => gridSize * gridSize;

    // Creates the tiles. Call once at the start.
    public void Build()
    {
        if (environment == null)
            environment = FindObjectOfType<VirtualEnvironment>();

        // Inside a CAVE the grid fills the real floor.
        if (environment != null)
        {
            Vector3 dimensions = environment.dimensions;

            tileSize = Mathf.Min(dimensions.x, dimensions.z) / gridSize;
            transform.position = environment.transform.position;

            Debug.Log(
                "CAVE floor: " + dimensions.x + " m x " + dimensions.z +
                " m, tile = " + tileSize + " m"
            );
        }

        tileRenderers = new Renderer[gridSize, gridSize];
        colorIndices = new int[gridSize, gridSize];

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                GameObject tile = GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

                tile.name = "Tile_" + row + "_" + col;
                tile.transform.SetParent(transform, false);

                tile.transform.localPosition =
                    CellToLocal(new Vector2Int(row, col), 0f);

                tile.transform.localScale = new Vector3(
                    tileSize - gap,
                    0.05f,
                    tileSize - gap
                );

                Renderer tileRenderer = tile.GetComponent<Renderer>();

                tileRenderer.material =
                    new Material(Shader.Find("Standard"));

                tileRenderers[row, col] = tileRenderer;
            }
        }

        Shuffle();
        HideAll();
    }

    // Gives every tile a new random colour. Each colour appears equally often.
    public void Shuffle()
    {
        List<int> shuffled = new List<int>();

        for (int i = 0; i < TileCount; i++)
            shuffled.Add(i % ColorPalette.Count);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        for (int row = 0; row < gridSize; row++)
            for (int col = 0; col < gridSize; col++)
                colorIndices[row, col] = shuffled[row * gridSize + col];
    }

    public void ShowAll()
    {
        for (int row = 0; row < gridSize; row++)
            for (int col = 0; col < gridSize; col++)
                Reveal(new Vector2Int(row, col));
    }

    public void HideAll()
    {
        for (int row = 0; row < gridSize; row++)
            for (int col = 0; col < gridSize; col++)
                tileRenderers[row, col].material.color =
                    ColorPalette.Hidden;
    }

    // Shows the real colour of one tile.
    public void Reveal(Vector2Int cell)
    {
        tileRenderers[cell.x, cell.y].material.color =
            ColorPalette.Colors[colorIndices[cell.x, cell.y]];
    }

    public int GetColorIndex(Vector2Int cell)
    {
        return colorIndices[cell.x, cell.y];
    }

    // One number per tile, counted row by row.
    public int GetTileId(Vector2Int cell)
    {
        return cell.x * gridSize + cell.y;
    }

    // Finds the tile under a position in the room. False = outside the grid.
    public bool TryGetCell(Vector3 worldPosition, out Vector2Int cell)
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition);
        float centerOffset = (gridSize - 1) / 2f;

        int col = Mathf.RoundToInt(local.x / tileSize + centerOffset);
        int row = Mathf.RoundToInt(local.z / tileSize + centerOffset);

        cell = new Vector2Int(row, col);

        return row >= 0 && row < gridSize &&
               col >= 0 && col < gridSize;
    }

    // Is the position on this tile? A positive margin makes the tile bigger,
    // a negative margin makes it smaller.
    public bool IsInsideCell(
        Vector3 worldPosition,
        Vector2Int cell,
        float margin)
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition);
        Vector3 center = CellToLocal(cell, 0f);
        float half = tileSize / 2f + margin;

        return Mathf.Abs(local.x - center.x) <= half &&
               Mathf.Abs(local.z - center.z) <= half;
    }

    // Creates the small sphere that shows where a player stands.
    public Transform CreateMarker(string markerName, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        marker.name = markerName;
        marker.transform.SetParent(transform, false);
        marker.transform.localScale = Vector3.one * 0.3f;
        marker.GetComponent<Renderer>().material.color = color;

        // The marker must not block mouse clicks on the tiles.
        marker.GetComponent<Collider>().enabled = false;

        return marker.transform;
    }

    public void PlaceMarker(Transform marker, Vector2Int cell)
    {
        marker.localPosition = CellToLocal(cell, 0.23f);
    }

    // The board colours as text like "#FF0000", for the saved result.
    public string[] GetColorsAsHtml()
    {
        string[] result = new string[TileCount];

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                result[row * gridSize + col] = "#" +
                    ColorUtility.ToHtmlStringRGB(
                        ColorPalette.Colors[colorIndices[row, col]]
                    );
            }
        }

        return result;
    }

    private Vector3 CellToLocal(Vector2Int cell, float height)
    {
        float centerOffset = (gridSize - 1) / 2f;

        return new Vector3(
            (cell.y - centerOffset) * tileSize,
            height,
            (cell.x - centerOffset) * tileSize
        );
    }
}
