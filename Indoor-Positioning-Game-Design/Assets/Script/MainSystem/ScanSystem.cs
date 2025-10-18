using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using TMPro;

public class ScanSystem : MonoBehaviour
{
    public TextMeshProUGUI Xdisplay;
    public TextMeshProUGUI Zdisplay;
    private List<string> bssidList = new List<string>();  // store header BSSIDs
    private Dictionary<Vector2, Dictionary<string, float>> fingerprintDB
        = new Dictionary<Vector2, Dictionary<string, float>>();

    private AndroidJavaObject unityActivity;
    private AndroidJavaObject wifiManager;
    [SerializeField] private bool autoScanEnable = false;

    private void Start()
    {
        LoadFingerprintFile(Path.Combine(Application.persistentDataPath, "location_data.csv"));
    }

    void LoadFingerprintFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("Fingerprint file not found: " + path);
            return;
        }

        var lines = File.ReadAllLines(path);
        if (lines.Length < 2) return;

        var header = lines[0].Split(',');
        for (int i = 2; i < header.Length; i++)
        {
            bssidList.Add(header[i]);
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');

            float x = float.Parse(cols[0]);
            float z = float.Parse(cols[1]);
            var pos = new Vector2(x, z);
            var rssiMap = new Dictionary<string, float>();

            for (int j = 2; j < cols.Length; j++)
            {
                if (float.TryParse(cols[j], out float rssi))
                {
                    rssiMap[bssidList[j - 2]] = rssi;
                }
            }

            fingerprintDB[pos] = rssiMap;
        }

        Debug.Log($"Loaded {fingerprintDB.Count} positions with {bssidList.Count} BSSIDs");
    }

    public void scanOnce() {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            return;
        }

        if (unityActivity == null)
        {
            Debug.LogError("UnityActivity not found!");
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

        Invoke(nameof(UpdateResult), 2f);
    }

    private void UpdateResult() {
        AndroidJavaObject scanResults = wifiManager.Call<AndroidJavaObject>("getScanResults");
        if (scanResults == null)
        {
            Debug.Log("No scan results");
            return;
        }

        Dictionary<string, float> currentScan = new Dictionary<string, float>();

        int size = scanResults.Call<int>("size");
        Debug.Log($"Found {size} WiFi networks");

        for (int i = 0; i < size; i++)
        {
            AndroidJavaObject scanResult = scanResults.Call<AndroidJavaObject>("get", i);
            string bssid = scanResult.Get<string>("BSSID");
            int level = scanResult.Get<int>("level");
            Debug.Log($"[{i}] BSSID: {bssid}, RSSI: {level}");
            currentScan[bssid] = level;
        }

        var bestPos = EstimatePosition(currentScan);
        Debug.Log($"Estimated Position: {bestPos.x}, {bestPos.y}");

        if (Xdisplay != null)
            Xdisplay.text = "X: " + bestPos.x.ToString("F2");
        if (Zdisplay != null)
            Zdisplay.text = "Z: " + bestPos.y.ToString("F2");
    }

    Vector2 EstimatePosition(Dictionary<string, float> currentScan)
    {
        float minDistance = float.MaxValue;
        Vector2 bestPos = Vector2.zero;

        foreach (var kvp in fingerprintDB)
        {
            var refPos = kvp.Key;
            var refSignals = kvp.Value;

            float distance = 0;
            int matchCount = 0;

            // Compare only common BSSIDs
            foreach (var bssid in bssidList)
            {
                if (currentScan.ContainsKey(bssid) && refSignals.ContainsKey(bssid))
                {
                    float diff = currentScan[bssid] - refSignals[bssid];
                    distance += diff * diff;
                    matchCount++;
                }
            }

            if (matchCount > 0)
            {
                distance = Mathf.Sqrt(distance / matchCount);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestPos = refPos;
                }
            }
        }

        return bestPos;
    }

    IEnumerator autoScanCoroutine() {
        while (true)
        {
            scanOnce();
            yield return new WaitForSeconds(2f);
        }
    }

    public void autoScanButton() {
        autoScanEnable = !autoScanEnable;
        if (autoScanEnable)
        {
            StartCoroutine(autoScanCoroutine());
        }
        else 
        {
            StopCoroutine(autoScanCoroutine());
        }
    }
}

class PositionHandler
{
    private float posX;
    private float posZ;

    private void calculatePosition() {
        posX = 0;
        posZ = 0;
    }

    private float getPosX() {
        return posX;
    }

    public float getPosZ() 
    {
        return posZ;
    }
}