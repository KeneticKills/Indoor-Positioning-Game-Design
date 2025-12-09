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
    [SerializeField] private Transform TransformInputX;
    [SerializeField] private Transform TransformInputZ;

    private float InputX;
    private float InputZ;
    private bool write;

    private void Start()
    {
        write = true;

        if (useInput)
        {

            TMP_InputField inputXField = TransformInputX.GetComponent<TMP_InputField>();
            TMP_InputField inputZField = TransformInputZ.GetComponent<TMP_InputField>();
            string inputX = CleanInput(inputXField.text.Trim());
            string inputZ = CleanInput(inputZField.text.Trim());

            inputXField.text = "";
            inputZField.text = "";

            string locationData = "Input X : " + inputX + " ; Input Z : " + inputZ;
            Debug.Log(locationData);

            if (!float.TryParse(inputX, out InputX) ||
                !float.TryParse(inputZ, out InputZ))
            {
                Debug.LogWarning($"Invalid input X={inputX}, Z={inputZ}");
                return;
            }
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
    }

    public void Initiate(float InputX, float InputZ, bool write) {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }

        this.InputX = InputX;
        this.InputZ = InputZ;
        this.write = write;

        scanFunction();
    }

    private void getScanResult() 
    {
        if (!write) {
            writeFileFunction();
            return;
        }
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

        if (!write) {
            Invoke(nameof(getScanResult), 0.1f);
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

    private void writeFileFunction(List<(string bssid, int rssi)> scanResults)
    {
        string fileName = "location_data.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        bool fileExists = File.Exists(filePath);
        List<string> headers = new List<string> { "x", "z" };
        List<string> lines = new List<string>();
        int duplicateIndex = -1;

        if (fileExists)
        {
            lines = File.ReadAllLines(filePath).ToList();
            if (lines.Count > 0)
            {
                headers = lines[0].Split(',').ToList();
            }

            // Find duplicate row index (skip header at index 0)
            for (int i = 1; i < lines.Count; i++)
            {
                string[] parts = lines[i].Split(',');
                if (parts.Length >= 2 && parts[0] == InputX.ToString() && parts[1] == InputZ.ToString())
                {
                    duplicateIndex = i;
                    break;
                }
            }
        }

        // DELETE mode
        if (!write)
        {
            if (duplicateIndex == -1)
            {
                Debug.LogWarning($"No data found to delete at X,Z: {InputX},{InputZ}");
                return;
            }

            lines.RemoveAt(duplicateIndex);
            File.WriteAllLines(filePath, lines);
            Debug.Log($"Deleted row at X,Z: {InputX},{InputZ}");
            return;
        }

        // WRITE mode (add new or replace existing)
        foreach (var (bssid, _) in scanResults)
        {
            if (!headers.Contains(bssid))
                headers.Add(bssid);
        }

        Dictionary<string, string> rowData = headers.ToDictionary(h => h, h => "");
        rowData["x"] = InputX.ToString();
        rowData["z"] = InputZ.ToString();

        foreach (var (bssid, rssi) in scanResults)
        {
            rowData[bssid] = rssi.ToString();
        }

        string newLine = string.Join(",", headers.Select(h => rowData[h]));
        string headerLine = string.Join(",", headers);

        if (lines.Count == 0)
        {
            lines.Insert(0, headerLine);
        }
        else if (lines[0] != headerLine)
        {
            lines[0] = headerLine;
        }

        if (duplicateIndex != -1)
        {
            // REPLACE existing row
            lines[duplicateIndex] = newLine;
            Debug.Log($"Replaced row at X,Z: {InputX},{InputZ}");
        }
        else
        {
            // ADD new row
            lines.Add(newLine);
            Debug.Log($"Added new row at X,Z: {InputX},{InputZ}");
        }

        File.WriteAllLines(filePath, lines);
        Debug.Log(filePath);
        Debug.Log("Success");
    }

    // Overload for delete only (no scan results needed)
    private void writeFileFunction()
    {
        if (write)
        {
            Debug.LogWarning("Cannot write without scan results. Use writeFileFunction(scanResults, true) instead.");
            return;
        }

        string fileName = "location_data.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"File does not exist: {filePath}");
            return;
        }

        List<string> lines = File.ReadAllLines(filePath).ToList();
        int duplicateIndex = -1;

        // Find row index to delete (skip header at index 0)
        for (int i = 1; i < lines.Count; i++)
        {
            string[] parts = lines[i].Split(',');
            if (parts.Length >= 2 && parts[0] == InputX.ToString() && parts[1] == InputZ.ToString())
            {
                duplicateIndex = i;
                break;
            }
        }

        if (duplicateIndex == -1)
        {
            Debug.LogWarning($"No data found to delete at X,Z: {InputX},{InputZ}");
            return;
        }

        lines.RemoveAt(duplicateIndex);
        File.WriteAllLines(filePath, lines);
        Debug.Log($"Deleted row at X,Z: {InputX},{InputZ}");
        Debug.Log("Success");
    }
}
