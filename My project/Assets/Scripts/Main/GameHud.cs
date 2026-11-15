using UnityEngine;

// Shows the phase, the attempts and the score on the screen.
// (Later this moves to the front wall of the CAVE.)
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
        if (game == null)
            return;

        float panelY = Mathf.Max(10, Screen.height - 150);

        GUI.Box(new Rect(10, panelY, 300, 140), GetPhaseText());

        for (int player = 0; player < GameManager.PlayerCount; player++)
        {
            // The target colour is only known after it was chosen.
            bool targetKnown =
                game.State != GameState.ChoosingColors ||
                (player == 1 && game.ChoosingPlayer == 1);

            string target = targetKnown
                ? game.GetTargetColorName(player)
                : "?";

            GUI.Label(
                new Rect(25, panelY + 35 + player * 30, 275, 25),
                "Player " + (player + 1) + " - " + target +
                " | Attempts: " + game.GetAttempts(player) +
                "/" + GameManager.AttemptsPerPlayer
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

    string GetPhaseText()
    {
        switch (game.State)
        {
            case GameState.ChoosingColors:
                return "P" + (game.ChoosingPlayer + 1) +
                       ": TYPE A COLOUR FOR P" +
                       (2 - game.ChoosingPlayer);

            case GameState.Memorize:
                return "MEMORIZE THE COLORS";

            case GameState.Playing:
                return "PLAY - SELECT YOUR TILES";

            default:
                return "GAME OVER";
        }
    }
}
