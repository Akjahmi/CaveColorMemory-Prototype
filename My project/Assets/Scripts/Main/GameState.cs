// The four phases of one round.
public enum GameState
{
    // Each player chooses a colour for the other player.
    ChoosingColors,

    // All colours are visible for a few seconds.
    Memorize,

    // Colours are hidden; players select their tiles.
    Playing,

    // Both players used all attempts; the result is shown.
    GameOver
}
