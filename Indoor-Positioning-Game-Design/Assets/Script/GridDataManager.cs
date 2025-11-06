using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Manages saving and loading grid data to/from CSV files
/// </summary>
public class GridDataManager : MonoBehaviour
{
    [Header("File Settings")]
    public string fileName = "GridData.csv";
    
    private string FilePath
    {
        get { return Path.Combine(Application.persistentDataPath, fileName); }
    }

    /// <summary>
    /// Save all dot data to CSV file
    /// Format: X,Y,Data,IsVisited,CustomValue
    /// </summary>
    public void SaveGridDataToCSV(GridDotData[,] dotArray)
    {
        if (dotArray == null)
        {
            Debug.LogError("GridDataManager: Dot array is null!");
            return;
        }

        StringBuilder csv = new StringBuilder();
        
        // Header row
        csv.AppendLine("X,Y,Data,IsVisited,CustomValue");

        // Data rows
        int rows = dotArray.GetLength(0);
        int cols = dotArray.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                GridDotData dot = dotArray[y, x];
                if (dot != null)
                {
                    // Escape commas in data field
                    string escapedData = dot.storedData.Replace(",", ";");
                    csv.AppendLine($"{x},{y},{escapedData},{dot.isVisited},{dot.customValue}");
                }
            }
        }

        try
        {
            File.WriteAllText(FilePath, csv.ToString());
            Debug.Log($"Grid data saved to: {FilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save CSV: {e.Message}");
        }
    }

    /// <summary>
    /// Load grid data from CSV file
    /// </summary>
    public void LoadGridDataFromCSV(GridDotData[,] dotArray)
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogWarning($"CSV file not found at: {FilePath}");
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(FilePath);
            
            // Skip header
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                
                if (values.Length >= 5)
                {
                    int x = int.Parse(values[0]);
                    int y = int.Parse(values[1]);
                    string data = values[2].Replace(";", ","); // Unescape commas
                    bool isVisited = bool.Parse(values[3]);
                    int customValue = int.Parse(values[4]);

                    // Apply data to corresponding dot
                    if (y < dotArray.GetLength(0) && x < dotArray.GetLength(1))
                    {
                        GridDotData dot = dotArray[y, x];
                        if (dot != null)
                        {
                            dot.storedData = data;
                            dot.isVisited = isVisited;
                            dot.customValue = customValue;

                            // Update visual state
                            UnityEngine.UI.Image img = dot.GetComponent<UnityEngine.UI.Image>();
                            if (img != null)
                            {
                                img.color = isVisited ? Color.green : Color.red;
                            }
                        }
                    }
                }
            }

            Debug.Log($"Grid data loaded from: {FilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load CSV: {e.Message}");
        }
    }

    /// <summary>
    /// Delete the CSV file
    /// </summary>
    public void DeleteSaveFile()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
            Debug.Log("Save file deleted.");
        }
    }

    /// <summary>
    /// Check if save file exists
    /// </summary>
    public bool SaveFileExists()
    {
        return File.Exists(FilePath);
    }
}
