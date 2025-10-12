using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System.IO;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Android;

public class ScanFunction : MonoBehaviour
{
    private AndroidJavaObject unityActivity;
    private AndroidJavaObject wifiManager;

    [SerializeField] private bool useInput;
    [SerializeField] private Transform InputX;
    [SerializeField] private Transform InputZ;

    private void Start()
    {
        if (useInput)
        {

            TMP_InputField inputXField = InputX.GetComponent<TMP_InputField>();
            TMP_InputField inputZField = InputZ.GetComponent<TMP_InputField>();

            inputXField.text = "";
            inputZField.text = "";
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
    }

    private void getScanResult() 
    {
        AndroidJavaObject scanResults = wifiManager.Call<AndroidJavaObject>("getScanResults");
        if (scanResults == null)
        {
            Debug.Log("No scan results");
            return;
        }

        int size = scanResults.Call<int>("size");
        Debug.Log($"Found {size} WiFi networks");

        List<(string bssid, int rssi)> results = new List<(string bssid, int rssi)>();

        for (int i = 0; i < size; i++)
        {
            AndroidJavaObject scanResult = scanResults.Call<AndroidJavaObject>("get", i);
            string bssid = scanResult.Get<string>("BSSID");
            int level = scanResult.Get<int>("level");
            results.Add((bssid, level));
            Debug.Log($"[{i}] BSSID: {bssid}, RSSI: {level}");
        }

        writeFileFunction(results);
    }

    string CleanInput(string input)
    {
        return input.Replace("\u200B", ""); // remove ZERO WIDTH SPACE
    }

    public void onClick(string buttonType) {
        switch (buttonType.ToUpper()) {
            case "SCAN":
                scanFunction();
                break;
            case "RESULT":
                resultFunction();
                break;
            case "BACK":
                backFunction();
                break;
            default:
                Debug.Log(name + " used unavailable button type (" + buttonType + ")");
                break;
        }
    }

    private void backFunction()
    {
        SceneManager.LoadScene("PositionMenu");
    }

    private void resultFunction()
    {
        SceneManager.LoadScene("RSSIResultTab");
    }

    private void scanFunction() {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) 
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            return;
        }

        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }

        wifiManager = unityActivity.Call<AndroidJavaObject>("getSystemService", "wifi");
        if (wifiManager == null)
        {
            Debug.LogError("wifiManager is null");
            return;
        }

        bool started = wifiManager.Call<bool>("startScan");
        Debug.Log("Scan started: " + started);

        Invoke(nameof(getScanResult), 2f);
    }

    private void writeFileFunction(List<(string bssid, int rssi)> scanResults) {
        TMP_InputField inputXField = InputX.GetComponent<TMP_InputField>();
        TMP_InputField inputZField = InputZ.GetComponent<TMP_InputField>();
        string inputX = CleanInput(inputXField.text.Trim());
        string inputZ = CleanInput(inputZField.text.Trim());

        inputXField.text = "";
        inputZField.text = "";

        string locationData = "Input X : " + inputX + " ; Input Z : " + inputZ;
        Debug.Log(locationData);

        if (!float.TryParse(inputX, out float xValue) ||
            !float.TryParse(inputZ, out float zValue))
        {
            Debug.LogWarning($"Invalid input X={inputX}, Z={inputZ}");
            return;
        }

        string fileName = "location_data.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        bool fileExists = File.Exists(filePath);

        List<string> headers = new List<string> { "x", "z" };
        List<string> lines = new List<string>();

        if (fileExists)
        {
            bool duplicate = File.ReadLines(filePath).Any(line =>
            {
                string[] parts = line.Split(',');
                return parts.Length >= 2 && parts[0] == inputX && parts[1] == inputZ;
            });

            if (duplicate)
            {
                Debug.LogWarning($"Duplicate X,Z found: {inputX},{inputZ}");
                return;
            }

            lines = File.ReadAllLines(filePath).ToList();
            if (lines.Count > 0)
            {
                headers = lines[0].Split(',').ToList();
            }
        }

        foreach (var (bssid, _) in scanResults)
        {
            if (!headers.Contains(bssid))
                headers.Add(bssid);
        }

        Dictionary<string, string> rowData = headers.ToDictionary(h => h, h => "");
        rowData["x"] = inputX;
        rowData["z"] = inputZ;

        foreach (var (bssid, rssi) in scanResults)
        {
            rowData[bssid] = rssi.ToString();
        }

        string line = string.Join(",", headers.Select(h => rowData[h]));

        if (lines.Count == 0 || lines[0] != string.Join(",", headers))
        {
            lines.Insert(0, string.Join(",", headers));
        }

        lines.Add(line);
        File.WriteAllLines(filePath, lines);

        Debug.Log(filePath);
        Debug.Log("Success");
    }
}
