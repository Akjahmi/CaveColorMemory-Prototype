
using UnityEngine;
using HTW.CAVE.Kinect;
using Windows.Kinect;

public class CaveKinectBridge : MonoBehaviour
{
    public KinectTracker tracker;

    // Keep one tracking ID for each player.
    private ulong[] playerIds = new ulong[2];

    private float nextLogTime;

    void Start()
    {
        if (tracker == null)
        {
            tracker = FindObjectOfType<KinectTracker>();
        }

        if (tracker == null)
        {
            Debug.Log(
                "No KinectTracker assigned. " +
                "Waiting for CAVE integration."
            );
        }
    }

    void Update()
    {
        if (tracker == null || tracker.actors == null)
            return;

        // Assign up to two tracked people.
        foreach (KinectActor actor in tracker.actors)
        {
            if (actor == null || actor.trackingId == 0)
                continue;

            bool alreadyAssigned =
                actor.trackingId == playerIds[0] ||
                actor.trackingId == playerIds[1];

            if (alreadyAssigned)
                continue;

            for (int i = 0; i < 2; i++)
            {
                if (playerIds[i] == 0)
                {
                    playerIds[i] = actor.trackingId;
                    break;
                }
            }
        }

        // Print a report every second.
        if (Time.time < nextLogTime)
            return;

        nextLogTime = Time.time + 1f;

        for (int player = 0; player < 2; player++)
        {
            KinectActor actor = FindPlayer(playerIds[player]);

            if (actor == null)
            {
                Debug.Log(
                    "Player " + (player + 1) +
                    ": not currently tracked"
                );

                continue;
            }

            KinectJoint leftFoot =
                actor.GetJoint(JointType.FootLeft);

            KinectJoint rightFoot =
                actor.GetJoint(JointType.FootRight);

            if (leftFoot.trackingState != TrackingState.Tracked ||
                rightFoot.trackingState != TrackingState.Tracked)
            {
                Debug.Log(
                    "Player " + (player + 1) +
                    ": foot tracking unavailable or inferred"
                );
                continue;
            }

            Debug.Log(
                "Player " + (player + 1) +
                " | Left foot: " + leftFoot.position +
                " | Right foot: " + rightFoot.position
            );
        }
    }

    KinectActor FindPlayer(ulong trackingId)
    {
        if (trackingId == 0)
            return null;

        foreach (KinectActor actor in tracker.actors)
        {
            if (actor != null && actor.trackingId == trackingId)
                return actor;
        }

        return null;
    }
}
