using UnityEngine;
using System.IO;

public static class ReadAuthData
{
    public static AuthData LoadAuthData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "auth.json");

        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            AuthData authData = JsonUtility.FromJson<AuthData>(jsonContent);
            return authData;
        }
        else
        {
            Debug.LogError("auth.json file not found.");
            return null;
        }
    }
}
