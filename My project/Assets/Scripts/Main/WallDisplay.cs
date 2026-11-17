using UnityEngine;
using HTW.CAVE;

// Shows the game text on the front wall of the CAVE.
public class WallDisplay : MonoBehaviour
{
    // The wall that shows the text: Front, Back, Left or Right.
    public VirtualScreen.Kind wall = VirtualScreen.Kind.Front;

    // Height of the text above the floor in metres.
    public float height = 1.6f;

    // Size of all text. 1 = normal, 1.5 = half as big again.
    public float textScale = 1f;

    // The normal PC camera. It is switched off in the real CAVE.
    public Camera pcCamera;

    private GameManager game;
    private GameHud hud;
    private KeyboardInput keyboard;
    private VoiceInput voice;

    private Font font;
    private TextMesh title;
    private TextMesh players;
    private TextMesh hint;

    void Start()
    {
        game = FindObjectOfType<GameManager>();
        hud = FindObjectOfType<GameHud>();
        keyboard = FindObjectOfType<KeyboardInput>();
        voice = FindObjectOfType<VoiceInput>();

        VirtualEnvironment environment =
            FindObjectOfType<VirtualEnvironment>();

        // No CAVE in the scene: nothing to do.
        if (environment == null)
        {
            enabled = false;
            return;
        }

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // The floor and the ceiling cannot show the text.
        if (wall == VirtualScreen.Kind.Top ||
            wall == VirtualScreen.Kind.Bottom)
        {
            Debug.LogWarning("WallDisplay: choose a wall, not floor or top.");
            wall = VirtualScreen.Kind.Front;
        }

        // In the middle of the chosen wall, 1 cm behind the screen,
        // facing into the CAVE.
        Quaternion rotation = VirtualScreen.GetLocalRotation(wall);
        Vector3 position = VirtualScreen.GetLocalPosition(
            wall,
            environment.dimensions
        );

        position.y = height;
        position += rotation * Vector3.forward * 0.01f;

        Transform panel = new GameObject("Wall Display").transform;
        panel.SetParent(environment.transform, false);
        panel.localPosition = position;
        panel.localRotation = rotation;
        panel.localScale = Vector3.one * textScale;

        title = CreateText(panel, 0.35f, 0.15f);
        players = CreateText(panel, 0f, 0.09f);
        hint = CreateText(panel, -0.35f, 0.09f);

        // Several screens = the real CAVE: the PC camera would cover a wall.
        if (pcCamera != null && Display.displays.Length > 1)
            pcCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        title.text = hud.GetPhaseText();

        players.text =
            hud.GetPlayerLine(0) + "\n" + hud.GetPlayerLine(1);

        if (game.State == GameState.ChoosingColors)
        {
            string typed = keyboard != null ? keyboard.TypedText : "";

            hint.text =
                string.Join(" / ", ColorPalette.Names) + ":  " + typed;

            // Show what the microphone understood, so players know
            // whether to repeat the word.
            if (voice != null && voice.LastHeard != "")
                hint.text += "\nHeard: " + voice.LastHeard;
        }
        else if (game.State == GameState.GameOver)
        {
            hint.text =
                "P1: " + game.GetScore(0) + "/" +
                GameManager.AttemptsPerPlayer +
                "     P2: " + game.GetScore(1) + "/" +
                GameManager.AttemptsPerPlayer +
                "\nRaise one hand above your head for a new round";
        }
        else
        {
            hint.text = "";
        }
    }

    // y = height inside the panel, size = letter height in metres.
    TextMesh CreateText(Transform parent, float y, float size)
    {
        const int fontSize = 96;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = new Vector3(0f, y, 0f);

        TextMesh text = textObject.AddComponent<TextMesh>();
        text.font = font;
        text.fontSize = fontSize;
        text.characterSize = size * 10f / fontSize;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.white;

        textObject.GetComponent<MeshRenderer>().sharedMaterial =
            font.material;

        return text;
    }
}