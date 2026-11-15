using System.IO;
using UnityEngine;

// Writes the result of one round to a JSON file.
public static class RoundSaver
{
    public static void Save(RoundResultData data)
    {
        try
        {
            string folder = Path.Combine(
                Application.persistentDataPath,
                "RoundResults"
            );

            Directory.CreateDirectory(folder);

            string fileName = "round_" +
                System.DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") +
                "_" +
                System.Guid.NewGuid().ToString("N").Substring(0, 8) +
                ".json";

            string filePath = Path.Combine(folder, fileName);

            File.WriteAllText(filePath, JsonUtility.ToJson(data, true));

            Debug.Log("Round saved successfully: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to save round: " + ex.Message);
        }
    }
}
