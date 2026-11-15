using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The rules of the game: phases, colour choice, attempts and score.
// The floor is in Board, the keyboard in KeyboardInput, the text in GameHud,
// the Kinect in PlayerTracker.
public class GameManager : MonoBehaviour
{
    public const int PlayerCount = 2;
    public const int AttemptsPerPlayer = 4;

    public float previewDuration = 6f;

    public GameState State { get; private set; }

    // The player who is choosing a colour right now (0 or 1).
    public int ChoosingPlayer { get; private set; }

    public Board Board { get; private set; }

    // Marker colours. They are not game colours, so nobody mixes them up.
    private static readonly Color[] MarkerColors =
    {
        Color.white,
        Color.magenta
    };

    private Vector2Int[] playerCells = new Vector2Int[PlayerCount];
    private Transform[] playerMarkers = new Transform[PlayerCount];
    private int[] scores = new int[PlayerCount];
    private int[] targetColorIndex = new int[PlayerCount];

    private List<int>[] selections =
    {
        new List<int>(),
        new List<int>()
    };

    void Awake()
    {
        // Add the helper scripts if they are not in the scene yet.
        Board = GetOrAdd<Board>();
        GetOrAdd<KeyboardInput>();
        GetOrAdd<GameHud>();

        if (FindObjectOfType<PlayerTracker>() == null)
            gameObject.AddComponent<PlayerTracker>();
    }

    void Start()
    {
        Board.Build();

        for (int player = 0; player < PlayerCount; player++)
        {
            playerMarkers[player] = Board.CreateMarker(
                "Player_" + (player + 1),
                MarkerColors[player]
            );
        }

        StartRound();
    }

    // ------------------------------------------------------------------
    // Information for the other scripts
    // ------------------------------------------------------------------

    public int GetScore(int player) => scores[player];

    public int GetAttempts(int player) => selections[player].Count;

    public string GetTargetColorName(int player) =>
        ColorPalette.Names[targetColorIndex[player]];

    // ------------------------------------------------------------------
    // Round
    // ------------------------------------------------------------------

    public void StartRound()
    {
        StopAllCoroutines();

        for (int player = 0; player < PlayerCount; player++)
        {
            scores[player] = 0;
            selections[player].Clear();
        }

        // Players start in opposite corners.
        int last = Board.gridSize - 1;

        MovePlayerTo(0, new Vector2Int(0, 0));
        MovePlayerTo(1, new Vector2Int(last, last));

        Board.Shuffle();
        Board.HideAll();

        ChoosingPlayer = 0;
        State = GameState.ChoosingColors;

        Debug.Log("New round started!");
    }

    // A player names the colour the OTHER player must find.
    // The keyboard calls this now; voice recognition can call it later.
    public bool SubmitColorName(string colorName)
    {
        if (State != GameState.ChoosingColors)
            return false;

        int index = ColorPalette.FindIndex(colorName);

        if (index < 0)
        {
            Debug.Log("Unknown colour: " + colorName);
            return false;
        }

        targetColorIndex[1 - ChoosingPlayer] = index;

        if (ChoosingPlayer == 0)
        {
            ChoosingPlayer = 1;
        }
        else
        {
            StartCoroutine(MemorizeThenPlay());
        }

        return true;
    }

    IEnumerator MemorizeThenPlay()
    {
        State = GameState.Memorize;
        Board.ShowAll();

        yield return new WaitForSeconds(previewDuration);

        Board.HideAll();
        State = GameState.Playing;

        Debug.Log("Memory phase started!");
    }

    // ------------------------------------------------------------------
    // Players
    // ------------------------------------------------------------------

    // Moves a player's marker to the tile under a position in the room.
    // Used by the Kinect (hip position) and the keyboard test persons.
    public bool SetPlayerPosition(int player, Vector3 worldPosition)
    {
        if (!IsValidPlayer(player))
            return false;

        // Stay on the current tile until clearly on the next one.
        if (Board.IsInsideCell(
                worldPosition,
                playerCells[player],
                Board.hysteresis))
            return true;

        if (!Board.TryGetCell(worldPosition, out Vector2Int cell))
            return false;

        MovePlayerTo(player, cell);
        return true;
    }

    // Keyboard: moves a player one tile. The player stops at the grid edge.
    public void MovePlayerBy(int player, int rowChange, int colChange)
    {
        if (State != GameState.Playing || !IsValidPlayer(player))
            return;

        int last = Board.gridSize - 1;

        MovePlayerTo(player, new Vector2Int(
            Mathf.Clamp(playerCells[player].x + rowChange, 0, last),
            Mathf.Clamp(playerCells[player].y + colChange, 0, last)
        ));
    }

    // Selects the tile the player is standing on.
    public void SelectCurrentTile(int player)
    {
        if (State != GameState.Playing || !IsValidPlayer(player))
            return;

        if (selections[player].Count >= AttemptsPerPlayer)
            return;

        Vector2Int cell = playerCells[player];
        int tileId = Board.GetTileId(cell);

        // A player cannot select the same tile twice.
        if (selections[player].Contains(tileId))
        {
            Debug.Log("Tile already selected.");
            return;
        }

        selections[player].Add(tileId);
        Board.Reveal(cell);

        bool correct =
            Board.GetColorIndex(cell) == targetColorIndex[player];

        if (correct)
            scores[player]++;

        Debug.Log(
            "Player " + (player + 1) +
            (correct ? ": CORRECT!" : ": WRONG!") +
            " Score = " + scores[player]
        );

        if (selections[0].Count == AttemptsPerPlayer &&
            selections[1].Count == AttemptsPerPlayer)
        {
            FinishRound();
        }
    }

    // Mouse test: jump straight to a tile and select it.
    public bool TrySelectAtWorldPosition(int player, Vector3 worldPosition)
    {
        if (State != GameState.Playing || !IsValidPlayer(player))
            return false;

        if (!Board.TryGetCell(worldPosition, out Vector2Int cell))
            return false;

        // Ignore clicks in the gaps between tiles.
        if (!Board.IsInsideCell(worldPosition, cell, -Board.gap / 2f))
            return false;

        MovePlayerTo(player, cell);
        SelectCurrentTile(player);

        return true;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    void FinishRound()
    {
        State = GameState.GameOver;

        RoundSaver.Save(BuildResult());

        Debug.Log(
            "GAME OVER! Player 1: " + scores[0] + "/" + AttemptsPerPlayer +
            " | Player 2: " + scores[1] + "/" + AttemptsPerPlayer
        );
    }

    RoundResultData BuildResult()
    {
        RoundResultData data = new RoundResultData();

        data.completedAtUtc = System.DateTime.UtcNow.ToString("o");
        data.gridSize = Board.gridSize;
        data.previewDuration = previewDuration;
        data.boardColors = Board.GetColorsAsHtml();
        data.players = new PlayerRoundData[PlayerCount];

        for (int player = 0; player < PlayerCount; player++)
        {
            PlayerRoundData result = new PlayerRoundData();

            result.playerNumber = player + 1;
            result.targetColor = GetTargetColorName(player);
            result.score = scores[player];
            result.selectedTileIds = selections[player].ToArray();

            data.players[player] = result;
        }

        return data;
    }

    void MovePlayerTo(int player, Vector2Int cell)
    {
        playerCells[player] = cell;
        Board.PlaceMarker(playerMarkers[player], cell);
    }

    bool IsValidPlayer(int player)
    {
        return player >= 0 && player < PlayerCount;
    }

    T GetOrAdd<T>() where T : Component
    {
        T component = GetComponent<T>();

        if (component == null)
            component = gameObject.AddComponent<T>();

        return component;
    }
}
