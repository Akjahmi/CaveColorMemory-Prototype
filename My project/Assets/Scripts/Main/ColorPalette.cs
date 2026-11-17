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

    // German names, same order. Players may type or say these too.
    public static readonly string[] GermanNames =
    {
        "Rot", "Blau", "Grün", "Gelb"
    };

    // Colour of a tile while its real colour is hidden.
    public static readonly Color Hidden = Color.gray;

    public static int Count => Colors.Length;

    // Every word the speech recognition listens for.
    public static string[] AllNames
    {
        get
        {
            string[] all = new string[Names.Length + GermanNames.Length];
            Names.CopyTo(all, 0);
            GermanNames.CopyTo(all, Names.Length);
            return all;
        }
    }

    // Returns the colour number for an English or German name,
    // or -1 if the name is unknown. "Gruen" also counts as "Grün".
    public static int FindIndex(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return -1;

        string clean = name.Trim().Replace("ue", "ü").Replace("UE", "Ü");

        int index = IndexIn(Names, name.Trim());

        if (index < 0)
            index = IndexIn(GermanNames, clean);

        return index;
    }

    static int IndexIn(string[] list, string name)
    {
        return System.Array.FindIndex(
            list,
            n => n.Equals(name,
                System.StringComparison.OrdinalIgnoreCase)
        );
    }
}
