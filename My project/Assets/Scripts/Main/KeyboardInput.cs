using UnityEngine;

// Everything typed on the keyboard:
//   choosing phase : type a colour name, then Enter
//   playing phase  : Player 1 moves with W A S D and selects with Space,
//                    Player 2 moves with the arrow keys and selects with Enter
//   game over      : R = new round
public class KeyboardInput : MonoBehaviour
{
    // The letters typed so far (shown by GameHud).
    public string TypedText { get; private set; } = "";

    private GameManager game;

    void Awake()
    {
        game = GetComponent<GameManager>();

        if (game == null)
            game = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        switch (game.State)
        {
            case GameState.ChoosingColors:
                ReadTypedColor();
                break;

            case GameState.Playing:
                // Player 1: W A S D, one tile per key press.
                if (Input.GetKeyDown(KeyCode.W)) game.MovePlayerBy(0, 1, 0);
                if (Input.GetKeyDown(KeyCode.S)) game.MovePlayerBy(0, -1, 0);
                if (Input.GetKeyDown(KeyCode.A)) game.MovePlayerBy(0, 0, -1);
                if (Input.GetKeyDown(KeyCode.D)) game.MovePlayerBy(0, 0, 1);

                // Player 2: arrow keys, one tile per key press.
                if (Input.GetKeyDown(KeyCode.UpArrow)) game.MovePlayerBy(1, 1, 0);
                if (Input.GetKeyDown(KeyCode.DownArrow)) game.MovePlayerBy(1, -1, 0);
                if (Input.GetKeyDown(KeyCode.LeftArrow)) game.MovePlayerBy(1, 0, -1);
                if (Input.GetKeyDown(KeyCode.RightArrow)) game.MovePlayerBy(1, 0, 1);

                if (Input.GetKeyDown(KeyCode.Space))
                    game.SelectCurrentTile(0);

                if (Input.GetKeyDown(KeyCode.Return) ||
                    Input.GetKeyDown(KeyCode.KeypadEnter))
                    game.SelectCurrentTile(1);
                break;

            case GameState.GameOver:
                if (Input.GetKeyDown(KeyCode.R))
                    game.StartRound();
                break;
        }
    }

    void ReadTypedColor()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                if (TypedText.Length > 0)
                    TypedText =
                        TypedText.Substring(0, TypedText.Length - 1);
            }
            else if (c == '\n' || c == '\r')
            {
                game.SubmitColorName(TypedText);
                TypedText = "";
            }
            else
            {
                TypedText += c;
            }
        }
    }
}
