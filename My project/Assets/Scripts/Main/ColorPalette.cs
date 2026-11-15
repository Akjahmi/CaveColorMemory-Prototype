using UnityEngine;

// The standard colours of the game.
// The order of Names and Colors must match: 0 = Red, 1 = Blue, 2 = Green, 3 = Yellow.
public static class ColorPalette
{
    public static readonly string[] Names =
    {
        "Red", "Blue", "Green", "Yellow"
    };

    public static readonly Color[] Colors =
    {
        Color.red, Color.blue, Color.green, Color.yellow
    };

    // Colour of a tile while its real colour is hidden.
    public static readonly Color Hidden = Color.gray;

    public static int Count => Colors.Length;

    // Returns the colour number for a name, or -1 if the name is unknown.
    public static int FindIndex(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return -1;

        return System.Array.FindIndex(
            Names,
            n => n.Equals(name.Trim(),
                System.StringComparison.OrdinalIgnoreCase)
        );
    }
}
