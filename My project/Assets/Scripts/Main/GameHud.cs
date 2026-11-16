using UnityEngine;

// Shows the phase, the attempts and the score on the PC screen.
// In the real CAVE, WallDisplay shows the same text on the front wall.
public class GameHud : MonoBehaviour
{
    private GameManager game;
    private KeyboardInput keyboard;

    void Awake()
    {
        game = GetComponent<GameManager>();

        if (game == null)
            game = FindObjectOfType<GameManager>();

        keyboard = FindObjectOfType<KeyboardInput>();
    }

    void OnGUI()
    {
        // In the real CAVE (several screens) the wall shows the text instead.
        if (game == null || Display.displays.Length > 1)
            return;

        float panelY = Mathf.Max(10, Screen.height - 150);

        GUI.Box(new Rect(10, panelY, 300, 140), GetPhaseText());

        for (int player = 0; player < GameManager.PlayerCount; player++)
        {
            GUI.Label(
                new Rect(25, panelY + 35 + player * 30, 275, 25),
                GetPlayerLine(player)
            );
        }

        if (game.State == GameState.ChoosingColors)
        {
            string typed = keyboard != null ? keyboard.TypedText : "";

            GUI.Label(
                new Rect(25, panelY + 105, 275, 30),
                string.Join(" / ", ColorPalette.Names) + ":  " + typed
            );
        }

        if (game.State == GameState.GameOver)
        {
            GUI.Label(
                new Rect(25, panelY + 105, 275, 30),
                "FINAL SCORE: P1 " + game.GetScore(0) +
                "/" + GameManager.AttemptsPerPlayer +
                " | P2 " + game.GetScore(1) +
                "/" + GameManager.AttemptsPerPlayer +
                "   (R = new round)"
            );
        }
    }

    public string GetPhaseText()
    {
        switch (game.State)
        {
            case GameState.ChoosingColors:
                return "P" + (game.ChoosingPlayer + 1) +
                       ": SAY OR TYPE A COLOUR FOR P" +
                       (2 - game.ChoosingPlayer);

            case GameState.Memorize:
                return "MEMORIZE THE COLORS";

            case GameState.Playing:
                return "PLAY - SELECT YOUR TILES";

            default:
                return "GAME OVER";
        }
    }
    // One line of text for a player, e.g. "Player 1 - Blue | Attempts: 2/4".
    public string GetPlayerLine(int player)
    {
        // The target colour is only known after it was chosen.
        bool targetKnown =
            game.State != GameState.ChoosingColors ||
            (player == 1 && game.ChoosingPlayer == 1);

        string target = targetKnown
            ? game.GetTargetColorName(player)
            : "?";

        return "Player " + (player + 1) + " - " + target +
               " | Attempts: " + game.GetAttempts(player) +
               "/" + GameManager.AttemptsPerPlayer;
    }
}
