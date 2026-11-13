
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public int gridSize = 4;
    public float tileSize = 1f;
    public float gap = 0.08f;
    public float previewDuration = 6f;

    private Renderer[,] tileRenderers;
    private Color[,] originalColors;

    private Vector2Int[] playerCells = new Vector2Int[2];
    private GameObject[] playerMarkers = new GameObject[2];

    private HashSet<int>[] selections =
    {
        new HashSet<int>(),
        new HashSet<int>()
    };

    private bool canSelect = false;

    private int[] scores = new int[2];

    private bool roundFinished = false;

    private const int AttemptsPerPlayer = 4;


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

        // Create a balanced, shuffled list of colors.
        List<int> colorIndices = new List<int>();

        for (int i = 0; i < gridSize * gridSize; i++)
        {
            colorIndices.Add(i % colors.Length);
        }

        for (int i = colorIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = colorIndices[i];
            colorIndices[i] = colorIndices[j];
            colorIndices[j] = temp;
        }

        // Generate the 4x4 grid.
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

                int colorIndex =
                    colorIndices[row * gridSize + col];

                Color chosenColor = colors[colorIndex];

                Material material = new Material(
                    Shader.Find("Standard")
                );

                material.color = chosenColor;

                Renderer renderer =
                    tile.GetComponent<Renderer>();

                renderer.material = material;

                tileRenderers[row, col] = renderer;
                originalColors[row, col] = chosenColor;
            }
        }

        // Set initial player positions.
        playerCells[0] = new Vector2Int(0, 0);
        playerCells[1] = new Vector2Int(
            gridSize - 1,
            gridSize - 1
        );

        CreatePlayer(0, Color.red);
        CreatePlayer(1, Color.blue);

        StartCoroutine(HideColorsAfterDelay());
    }

    void CreatePlayer(int player, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        marker.name = "Player_" + (player + 1);
        marker.transform.SetParent(transform, false);
        marker.transform.localScale = Vector3.one * 0.3f;

        marker.GetComponent<Renderer>().material.color =
            color;

        marker.GetComponent<Collider>().enabled = false;

        playerMarkers[player] = marker;
        UpdateMarkerPosition(player);
    }

    void UpdateMarkerPosition(int player)
    {
        int row = playerCells[player].x;
        int col = playerCells[player].y;

        float x = (col - (gridSize - 1) / 2f) * tileSize;
        float z = (row - (gridSize - 1) / 2f) * tileSize;

        playerMarkers[player].transform.localPosition =
            new Vector3(x, 0.23f, z);
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

        canSelect = true;

        Debug.Log("Memory phase started!");
    }

    void Update()
    {

        if (roundFinished && Input.GetKeyDown(KeyCode.R))
        {
            RestartRound();
            return;
        }


        if (!canSelect)
            return;

        // Player 1: WASD + Space
        if (Input.GetKeyDown(KeyCode.W))
            MovePlayer(0, 1, 0);

        if (Input.GetKeyDown(KeyCode.S))
            MovePlayer(0, -1, 0);

        if (Input.GetKeyDown(KeyCode.A))
            MovePlayer(0, 0, -1);

        if (Input.GetKeyDown(KeyCode.D))
            MovePlayer(0, 0, 1);

        if (Input.GetKeyDown(KeyCode.Space))
            SelectTile(0);

        // Player 2: Arrow keys + Enter
        if (Input.GetKeyDown(KeyCode.UpArrow))
            MovePlayer(1, 1, 0);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            MovePlayer(1, -1, 0);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MovePlayer(1, 0, -1);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            MovePlayer(1, 0, 1);

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SelectTile(1);
        }
    }

    void MovePlayer(int player, int rowChange, int colChange)
    {
        int newRow = Mathf.Clamp(
            playerCells[player].x + rowChange,
            0,
            gridSize - 1
        );

        int newCol = Mathf.Clamp(
            playerCells[player].y + colChange,
            0,
            gridSize - 1
        );

        playerCells[player] = new Vector2Int(
            newRow,
            newCol
        );

        UpdateMarkerPosition(player);
    }


    void SelectTile(int player)
    {
        if (!canSelect || roundFinished)
            return;

        if (selections[player].Count >= AttemptsPerPlayer)
            return;

        int row = playerCells[player].x;
        int col = playerCells[player].y;

        int tileId = row * gridSize + col;

        // Prevent selecting the same tile twice.
        if (!selections[player].Add(tileId))
        {
            Debug.Log("Tile already selected.");
            return;
        }

        // Player 1 = red, Player 2 = blue.
        Color targetColor = player == 0
            ? Color.red
            : Color.blue;

        bool correct = originalColors[row, col] == targetColor;

        if (correct)
        {
            scores[player]++;

            Debug.Log(
                "Player " + (player + 1) +
                ": CORRECT! Score = " + scores[player]
            );
        }
        else
        {
            Debug.Log(
                "Player " + (player + 1) +
                ": WRONG! Row " + (row + 1) +
                ", Column " + (col + 1)
            );
        }

        // End the round when both players finish.
        if (selections[0].Count == AttemptsPerPlayer &&
            selections[1].Count == AttemptsPerPlayer)
        {
            roundFinished = true;
            canSelect = false;

            Debug.Log(
                "GAME OVER! Player 1: " + scores[0] +
                "/4 | Player 2: " + scores[1] + "/4"
            );
        }
    }


    void RestartRound()
    {
        StopAllCoroutines();

        canSelect = false;
        roundFinished = false;

        // Reset both players.
        for (int player = 0; player < 2; player++)
        {
            scores[player] = 0;
            selections[player].Clear();
        }

        playerCells[0] = new Vector2Int(0, 0);
        playerCells[1] = new Vector2Int(
            gridSize - 1,
            gridSize - 1
        );

        UpdateMarkerPosition(0);
        UpdateMarkerPosition(1);

        // Generate balanced random colors.
        List<int> colorIndices = new List<int>();

        for (int i = 0; i < gridSize * gridSize; i++)
        {
            colorIndices.Add(i % colors.Length);
        }

        // Shuffle the colors.
        for (int i = colorIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            int temp = colorIndices[i];
            colorIndices[i] = colorIndices[j];
            colorIndices[j] = temp;
        }

        // Assign new colors to the existing tiles.
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                int index = row * gridSize + col;
                Color newColor = colors[colorIndices[index]];

                originalColors[row, col] = newColor;

                tileRenderers[row, col].material.color =
                    newColor;
            }
        }

        StartCoroutine(HideColorsAfterDelay());

        Debug.Log("New round started!");
    }



    void OnGUI()
    {
        if (tileRenderers == null)
            return;

        string phase;

        if (roundFinished)
            phase = "GAME OVER";
        else if (canSelect)
            phase = "PLAY - SELECT YOUR TILES";
        else
            phase = "MEMORIZE THE COLORS";

        float panelY = Mathf.Max(10, Screen.height - 150);

        GUI.Box(new Rect(10, panelY, 300, 140), phase);

        GUI.Label(
            new Rect(25, panelY + 35, 275, 25),
            "Player 1 - RED | Attempts: " +
            selections[0].Count + "/4"
        );

        GUI.Label(
            new Rect(25, panelY + 65, 275, 25),
            "Player 2 - BLUE | Attempts: " +
            selections[1].Count + "/4"
        );

        if (roundFinished)
        {
            GUI.Label(
                new Rect(25, panelY + 105, 275, 30),
                "FINAL SCORE: RED " + scores[0] +
                "/4 | BLUE " + scores[1] + "/4"
            );
        }
    }

}
