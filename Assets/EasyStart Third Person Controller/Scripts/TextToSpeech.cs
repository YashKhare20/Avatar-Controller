using System.Collections;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField, Tooltip(("Your OpenAI API key. If you use a restricted key, please ensure that it has permissions for /v1/audio."))] private string openAiApiKey;

    private const string OpenAiApiUrl = "https://api.openai.com/v1/audio/speech"; // Verify this endpoint
    private readonly string outputFormat = "mp3";

    [System.Serializable]
    private class TTSPayload
    {
        public string model;
        public string input;
        public string voice;
        public string response_format;
        public float speed;
    }

    public async Task<byte[]> RequestTextToSpeech(string text, string model = "tts-1", string voice = "alloy", float speed = 1f)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogError("Input text is empty or null.");
            return null;
        }

        Debug.Log("Sending new request to OpenAI TTS.");
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

        TTSPayload payload = new TTSPayload
        {
            model = model,
            input = text,
            voice = voice.ToLower(),
            response_format = this.outputFormat,
            speed = speed
        };

        string jsonPayload = JsonUtility.ToJson(payload);
        Debug.Log("Payload: " + jsonPayload);

        var startTime = Time.realtimeSinceStartup;
        var httpResponse = await httpClient.PostAsync(
            OpenAiApiUrl,
            new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        );
        var endTime = Time.realtimeSinceStartup;
        Debug.Log($"Text-to-speech API call latency: {endTime - startTime} seconds");

        string responseString = await httpResponse.Content.ReadAsStringAsync();
        Debug.Log("Response: " + responseString);

        if (httpResponse.IsSuccessStatusCode)
        {
            byte[] response = await httpResponse.Content.ReadAsByteArrayAsync();
            return response;
        }

        Debug.LogError("Error: " + httpResponse.StatusCode);
        Debug.LogError("Error details: " + responseString);
        return null;
    }

    public void MakeAudioRequest(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Debug.LogError("Message for TTS is empty or null.");
            return;
        }
        StartCoroutine(SendAudioRequestCoroutine(message));
    }

    private IEnumerator SendAudioRequestCoroutine(string message)
    {
        var requestDataTask = RequestTextToSpeech(message);
        while (!requestDataTask.IsCompleted)
        {
            yield return null;
        }

        byte[] audioBytes = requestDataTask.Result;

        if (audioBytes == null)
        {
            Debug.LogError("Failed to get audio bytes from TTS request.");
            yield break;
        }

        // Write the audio file to disk
        string filePath = Path.Combine(Application.persistentDataPath, "speech.mp3");
        File.WriteAllBytes(filePath, audioBytes);

        // Load the audio file into Unity
        using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"TTS Error: {www.error}");
                yield break;
            }

            var clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void SetAPIKey(string openAiApiKey)
    {
        this.openAiApiKey = openAiApiKey;
    }
}
