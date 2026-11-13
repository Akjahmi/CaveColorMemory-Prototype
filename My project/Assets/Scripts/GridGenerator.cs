
using System.Collections;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public int gridSize = 4;
    public float tileSize = 1f;
    public float gap = 0.08f;

    public float previewDuration = 6f;

    private Renderer[,] tileRenderers;
    private Color[,] originalColors;

    private Color[] colors =
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

    void Start()
    {
        tileRenderers = new Renderer[gridSize, gridSize];
        originalColors = new Color[gridSize, gridSize];

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                GameObject tile = GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

                tile.name = "Tile_" + row + "_" + col;
                tile.transform.SetParent(transform, false);

                float x = (col - (gridSize - 1) / 2f) * tileSize;
                float z = (row - (gridSize - 1) / 2f) * tileSize;

                tile.transform.localPosition =
                    new Vector3(x, 0f, z);

                tile.transform.localScale =
                    new Vector3(
                        tileSize - gap,
                        0.05f,
                        tileSize - gap
                    );

                int randomIndex = Random.Range(0, colors.Length);
                Color selectedColor = colors[randomIndex];

                Material material = new Material(
                    Shader.Find("Standard")
                );

                material.color = selectedColor;

                Renderer tileRenderer =
                    tile.GetComponent<Renderer>();

                tileRenderer.material = material;

                tileRenderers[row, col] = tileRenderer;
                originalColors[row, col] = selectedColor;
            }
        }

        StartCoroutine(HideColorsAfterDelay());
    }

    IEnumerator HideColorsAfterDelay()
    {
        yield return new WaitForSeconds(previewDuration);

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                tileRenderers[row, col].material.color =
                    Color.gray;
            }
        }

        Debug.Log("Memory phase finished. Colors hidden.");
    }
}
