// Windows speech recognition only exists on Windows:
// in the Windows editor and in Windows builds, never in the macOS or Linux editor.
#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX && !UNITY_EDITOR_LINUX) || UNITY_EDITOR_WIN
#define WINDOWS_SPEECH
#endif

using UnityEngine;
#if WINDOWS_SPEECH
using UnityEngine.Windows.Speech;
#endif

// Lets the players say the colour name instead of typing it.
// Uses the offline speech recognition of Windows and the default microphone.
// On other systems, or without a microphone, it does nothing and typing still works.
public class VoiceInput : MonoBehaviour
{
    public bool useVoice = true;

    // The last colour heard in this choosing phase (shown on the wall).
    public string LastHeard { get; private set; } = "";

    public bool IsRunning { get; private set; }

    private GameManager game;

#if WINDOWS_SPEECH
    private KeywordRecognizer recognizer;
#endif

    void Start()
    {
        game = FindObjectOfType<GameManager>();

#if WINDOWS_SPEECH
        if (!useVoice)
            return;

        try
        {
            if (!PhraseRecognitionSystem.isSupported)
            {
                Debug.LogWarning(
                    "Voice input: speech recognition is not supported " +
                    "on this computer. Type the colours instead."
                );
                return;
            }

            // Only the colour names (English and German) are listened for.
            // Medium = fewer false words. Use ConfidenceLevel.Low if
            // the CAVE is noisy and correct words are missed.
            recognizer = new KeywordRecognizer(
                ColorPalette.AllNames,
                ConfidenceLevel.Medium
            );

            recognizer.OnPhraseRecognized += OnPhraseRecognized;
            recognizer.Start();

            IsRunning = true;

            Debug.Log(
                "Voice input running: " +
                string.Join(", ", ColorPalette.AllNames)
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(
                "Voice input could not start: " + ex.Message +
                ". Type the colours instead."
            );
        }
#endif
    }

    void Update()
    {
        // Forget the last word once the choosing phase is over.
        if (game != null && game.State != GameState.ChoosingColors)
            LastHeard = "";
    }

#if WINDOWS_SPEECH
    void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        // Colour names said during the game are ignored.
        if (game == null || game.State != GameState.ChoosingColors)
            return;

        LastHeard = args.text;

        Debug.Log("Heard: " + args.text + " (" + args.confidence + ")");

        game.SubmitColorName(args.text);
    }
#endif

    void OnDestroy()
    {
#if WINDOWS_SPEECH
        if (recognizer == null)
            return;

        recognizer.OnPhraseRecognized -= OnPhraseRecognized;

        if (recognizer.IsRunning)
            recognizer.Stop();

        recognizer.Dispose();
        recognizer = null;
#endif
    }
}
