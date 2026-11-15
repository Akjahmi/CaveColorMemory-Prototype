using UnityEngine;
using HTW.CAVE;

// Shows the game text on the front wall of the CAVE.
public class WallDisplay : MonoBehaviour
{
    // Height of the text above the floor in metres.
    public float height = 1.6f;

    // The normal PC camera. It is switched off in the real CAVE.
    public Camera pcCamera;

    private GameManager game;
    private GameHud hud;
    private KeyboardInput keyboard;

    private Font font;
    private TextMesh title;
    private TextMesh players;
    private TextMesh hint;

    void Start()
    {
        game = FindObjectOfType<GameManager>();
        hud = FindObjectOfType<GameHud>();
        keyboard = FindObjectOfType<KeyboardInput>();

        VirtualEnvironment environment =
            FindObjectOfType<VirtualEnvironment>();

        // No CAVE in the scene: nothing to do.
        if (environment == null)
        {
            enabled = false;
            return;
        }

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Just behind the front wall, in the middle.
        Transform panel = new GameObject("Wall Display").transform;
        panel.SetParent(environment.transform, false);
        panel.localPosition = new Vector3(
            0f,
            height,
            environment.dimensions.z * 0.5f + 0.01f
        );

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
            hint.text =
                string.Join(" / ", ColorPalette.Names) +
                ":  " + keyboard.TypedText;
        }
        else if (game.State == GameState.GameOver)
        {
            hint.text =
                "P1: " + game.GetScore(0) + "/" +
                GameManager.AttemptsPerPlayer +
                "     P2: " + game.GetScore(1) + "/" +
                GameManager.AttemptsPerPlayer;
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