using UnityEngine;
using HTW.CAVE.Kinect;
using JointType = Windows.Kinect.JointType;
using TrackingState = Windows.Kinect.TrackingState;

// Tells the game where each player stands in the CAVE,
// using the hip (SpineBase) of up to two persons tracked by the Kinect.
// A player selects a tile by standing on it for a moment.
// After game over, raising one hand above the head starts a new round.
// Without a Kinect this script does nothing; the keyboard is in KeyboardInput.
public class PlayerTracker : MonoBehaviour
{
    public GameManager game;

    // Leave empty: the tracker of the CAVE is found automatically.
    public KinectTracker tracker;

    // Kinect tracking ID of each player. 0 = free slot.
    private ulong[] playerIds = new ulong[GameManager.PlayerCount];

    // How long (s) a player must stand on a tile to select it.
    public float dwellTime = 1.2f;

    private Vector2Int[] lastCell = new Vector2Int[GameManager.PlayerCount];
    private float[] cellSince = new float[GameManager.PlayerCount];
    private bool[] doneHere = new bool[GameManager.PlayerCount];

    [Header("New round gesture")]
    // How far (m) a hand must be above the head.
    public float handAboveHead = 0.1f;

    // How long (s) the hand must stay up.
    public float raiseHoldTime = 1f;

    // Pause (s) after game over before the gesture counts,
    // so a cheering player does not start the next round by accident.
    public float restartDelay = 2f;

    private float gameOverSince = -1f;
    private float raisedSince = -1f;

    void Start()
    {
        if (game == null)
            game = FindObjectOfType<GameManager>();

        if (tracker == null)
            tracker = FindObjectOfType<KinectTracker>(true);

        if (tracker == null)
            Debug.Log("No KinectTracker found. Use the keyboard to play.");
    }

    void Update()
    {
        AssignPlayers();

        for (int player = 0; player < GameManager.PlayerCount; player++)
        {
            KinectActor actor = FindActor(playerIds[player]);

            if (actor == null)
                continue;

            var hip = actor.GetJoint(JointType.SpineBase);

            if (hip.trackingState == TrackingState.NotTracked)
                continue;

            game.SetPlayerPosition(player, hip.position);
            UpdateDwell(player);
        }

        UpdateNewRoundGesture();
    }

    // After game over: any player raises a hand above the head = new round.
    void UpdateNewRoundGesture()
    {
        if (game.State != GameState.GameOver)
        {
            gameOverSince = -1f;
            raisedSince = -1f;
            return;
        }

        if (gameOverSince < 0f)
            gameOverSince = Time.time;

        if (Time.time - gameOverSince < restartDelay)
            return;

        bool anyRaised = false;

        for (int player = 0; player < GameManager.PlayerCount; player++)
        {
            KinectActor actor = FindActor(playerIds[player]);

            if (actor != null && IsHandRaised(actor))
                anyRaised = true;
        }

        if (!anyRaised)
        {
            raisedSince = -1f;
            return;
        }

        if (raisedSince < 0f)
            raisedSince = Time.time;

        if (Time.time - raisedSince >= raiseHoldTime)
        {
            raisedSince = -1f;
            game.StartRound();
        }
    }

    bool IsHandRaised(KinectActor actor)
    {
        KinectJoint head = actor.GetJoint(JointType.Head);

        if (head.trackingState == TrackingState.NotTracked)
            return false;

        float limit = head.position.y + handAboveHead;

        return IsAbove(actor.GetJoint(JointType.HandLeft), limit) ||
               IsAbove(actor.GetJoint(JointType.HandRight), limit);
    }

    // Only trust hands the Kinect really sees, not guessed ones.
    static bool IsAbove(KinectJoint joint, float height)
    {
        return joint.trackingState == TrackingState.Tracked &&
               joint.position.y > height;
    }

    // Selects the tile when the player has stood on it long enough.
    void UpdateDwell(int player)
    {
        Vector2Int cell = game.GetPlayerCell(player);

        // Outside the playing phase nothing is selected. The tile a player
        // stands on when play starts does not count until they step off it.
        if (game.State != GameState.Playing)
        {
            lastCell[player] = cell;
            doneHere[player] = true;
            return;
        }

        // Stepped onto a new tile: start the timer.
        if (cell != lastCell[player])
        {
            lastCell[player] = cell;
            cellSince[player] = Time.time;
            doneHere[player] = false;
            return;
        }

        if (!doneHere[player] &&
            Time.time - cellSince[player] >= dwellTime)
        {
            doneHere[player] = true;
            game.SelectCurrentTile(player);
        }
    }

    void AssignPlayers()
    {
        // Free the slot of a person the Kinect has lost.
        for (int i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] != 0 && FindActor(playerIds[i]) == null)
                playerIds[i] = 0;
        }

        if (tracker == null || tracker.actors == null)
            return;

        // Give every new person a free slot.
        foreach (KinectActor actor in tracker.actors)
        {
            if (actor == null || actor.trackingId == 0)
                continue;

            if (System.Array.IndexOf(playerIds, actor.trackingId) >= 0)
                continue;

            int freeSlot = System.Array.IndexOf(playerIds, 0UL);

            if (freeSlot >= 0)
                playerIds[freeSlot] = actor.trackingId;
        }
    }

    KinectActor FindActor(ulong trackingId)
    {
        if (trackingId == 0 || tracker == null || tracker.actors == null)
            return null;

        foreach (KinectActor actor in tracker.actors)
        {
            if (actor != null && actor.trackingId == trackingId)
                return actor;
        }

        return null;
    }
}
