using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class RemoteConfigLoader : MonoBehaviour
{
    [Header("Config Source")]
    public string configUrl = "https://example.com/weapons.json";

    [Header("Behaviour")]
    public int maxRetries = 3;
    public float timeoutSeconds = 10f;

    [Header("Debug")]
    public bool logDetailed = true;

    string cachePath;
    readonly List<Weapon> defaultWeapons = new()
    {
        new("pistol", 10, 0.5f),
        new("rifle", 25, 0.2f)
    };

    void Awake() => cachePath = Path.Combine(Application.persistentDataPath, "weapons_cache.json");
    IEnumerator Start() => StartCoroutine(LoadConfigWithRetries());

    IEnumerator LoadConfigWithRetries()
    {
        float delay = 0.5f;
        for (int i = 0; i < maxRetries; i++)
        {
            if (logDetailed) Debug.Log($"Attempt {i + 1} to download config");

            bool success = false;
            yield return StartCoroutine(DownloadAndProcess(v => success = v));
            if (success) yield break;

            yield return new WaitForSeconds(delay);
            delay *= 2;
        }

        ApplyWeapons(TryLoadCache() ?? defaultWeapons);
    }

    IEnumerator DownloadAndProcess(Action<bool> callback)
    {
        using var www = UnityWebRequest.Get(configUrl);
        www.timeout = (int)timeoutSeconds;
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Download failed: {www.error}");
            callback(false);
            yield break;
        }

        var weapons = ConfigParser.ParseWeapons(www.downloadHandler.text);
        if (weapons == null)
        {
            callback(false);
            yield break;
        }

        File.WriteAllText(cachePath, www.downloadHandler.text);
        if (logDetailed) Debug.Log($"Config cached to {cachePath}");

        ApplyWeapons(weapons);
        callback(true);
    }

    List<Weapon> TryLoadCache()
    {
        if (!File.Exists(cachePath)) return null;

        try
        {
            var cached = File.ReadAllText(cachePath);
            var weapons = ConfigParser.ParseWeapons(cached);
            if (weapons != null)
            {
                if (logDetailed) Debug.Log("Using cached config");
                return weapons;
            }
            Debug.LogWarning("Cache invalid");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Cache read failed: {ex.Message}");
        }
        return null;
    }

    void ApplyWeapons(List<Weapon> weapons)
    {
        if (weapons?.Count == 0) return;
        Debug.Log($"Applying {weapons.Count} weapons:\n{string.Join("\n", weapons)}");
    }
}