using System.IO;
using UnityEngine;

public class CSVResultLoader : MonoBehaviour
{
    public GameObject resultTabPrefab;   // Assign prefab
    public Transform contentParent;      // Assign Content

    void Start()
    {
        // Locate the File
        string csvPath = Path.Combine(Application.persistentDataPath, "location_data.csv");

        Debug.Log("Loading results from: " + csvPath);
        LoadResultsFromCSV(csvPath);
    }

    void LoadResultsFromCSV(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV file not found at: " + filePath);
            return;
        }

        string[] lines = File.ReadAllLines(filePath);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            if (values.Length >= 2)
            {
                string xValue = values[0].Trim();
                string zValue = values[1].Trim();

                // Instantiate prefab
                GameObject newTab = Instantiate(resultTabPrefab, contentParent);
                newTab.transform.localScale = Vector3.one;

                // Apply values
                CSVResultPull ui = newTab.GetComponent<CSVResultPull>();
                if (ui != null)
                    ui.SetValues(xValue, zValue);
            }
        }
    }
}
