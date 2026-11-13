
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public int gridSize = 4;
    public float tileSize = 1f;
    public float gap = 0.08f;

    private Color[] colors =
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

    void Start()
    {
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                GameObject tile = GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

                tile.name = "Tile_" + row + "_" + col;
                tile.transform.SetParent(transform, false);

                float x = (col - 1.5f) * tileSize;
                float z = (row - 1.5f) * tileSize;

                tile.transform.localPosition =
                    new Vector3(x, 0, z);

                tile.transform.localScale =
                    new Vector3(
                        tileSize - gap,
                        0.05f,
                        tileSize - gap
                    );

                Material material = new Material(
                    Shader.Find("Standard")
                );

                material.color = colors[
                    Random.Range(0, colors.Length)
                ];

                tile.GetComponent<Renderer>().material =
                    material;
            }
        }
    }
}
