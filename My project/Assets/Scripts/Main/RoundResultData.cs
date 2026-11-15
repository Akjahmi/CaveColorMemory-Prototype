
using System;

[Serializable]
public class PlayerRoundData
{
    public int playerNumber;
    public string targetColor;
    public int score;
    public int[] selectedTileIds;
}

[Serializable]
public class RoundResultData
{
    public string completedAtUtc;
    public int gridSize;
    public float previewDuration;
    public string[] boardColors;
    public PlayerRoundData[] players;
}
