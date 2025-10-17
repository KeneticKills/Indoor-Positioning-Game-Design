using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class ScanSystem : MonoBehaviour
{

    private AndroidJavaObject unityActivity;
    private AndroidJavaObject wifiManager;
    [SerializeField] private bool autoScanEnable = false;

    public void scanOnce() {
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

        Invoke(nameof(UpdateResult), 2f);
    }

    private void UpdateResult() {
        AndroidJavaObject scanResults = wifiManager.Call<AndroidJavaObject>("getScanResults");
        if (scanResults == null)
        {
            Debug.Log("No scan results");
            return;
        }

        int size = scanResults.Call<int>("size");
        Debug.Log($"Found {size} WiFi networks");

        for (int i = 0; i < size; i++)
        {
            AndroidJavaObject scanResult = scanResults.Call<AndroidJavaObject>("get", i);
            string bssid = scanResult.Get<string>("BSSID");
            int level = scanResult.Get<int>("level");
            Debug.Log($"[{i}] BSSID: {bssid}, RSSI: {level}");
        }
    }

    IEnumerator autoScanCoroutine() {
        scanOnce();
        yield return new WaitForSeconds(2f);
    }

    public void autoScanButton() {
        autoScanEnable = !autoScanEnable;
        if (autoScanEnable) {
            StartCoroutine(autoScanCoroutine());
        }
    }
}
