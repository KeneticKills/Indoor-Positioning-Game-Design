using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Example script showing how to use the Grid Data Storage System
/// Attach this to a UI GameObject with buttons
/// </summary>
public class GridDataExample : MonoBehaviour
{
    [Header("References")]
    public MapDataWithStorage gridMap;

    [Header("UI Buttons (Optional)")]
    public Button saveButton;
    public Button loadButton;
    public Button resetButton;
    public Button fillTestDataButton;

    void Start()
    {
        // Setup button listeners if assigned
        if (saveButton) saveButton.onClick.AddListener(SaveData);
        if (loadButton) loadButton.onClick.AddListener(LoadData);
        if (resetButton) resetButton.onClick.AddListener(ResetData);
        if (fillTestDataButton) fillTestDataButton.onClick.AddListener(FillTestData);
    }

    /// <summary>
    /// Save current grid state to CSV
    /// </summary>
    public void SaveData()
    {
        if (gridMap != null)
        {
            gridMap.SaveGridData();
            Debug.Log("Grid data saved!");
        }
    }

    /// <summary>
    /// Load grid state from CSV
    /// </summary>
    public void LoadData()
    {
        if (gridMap != null)
        {
            gridMap.LoadGridData();
            Debug.Log("Grid data loaded!");
        }
    }

    /// <summary>
    /// Reset all data to default
    /// </summary>
    public void ResetData()
    {
        if (gridMap != null)
        {
            // Clear all dot data
            for (int y = 0; y < gridMap.rows; y++)
            {
                for (int x = 0; x < gridMap.cols; x++)
                {
                    GridDotData dot = gridMap.GetDotAt(x, y);
                    if (dot != null)
                    {
                        dot.storedData = "";
                        dot.isVisited = false;
                        dot.customValue = 0;

                        // Reset visual
                        Image img = dot.GetComponent<Image>();
                        if (img != null)
                        {
                            img.color = Color.red;
                        }
                    }
                }
            }

            // Delete save file
            if (gridMap.dataManager != null)
            {
                gridMap.dataManager.DeleteSaveFile();
            }

            Debug.Log("Grid data reset!");
        }
    }

    /// <summary>
    /// Fill grid with example test data
    /// </summary>
    public void FillTestData()
    {
        if (gridMap == null) return;

        int levelNumber = 1;

        for (int y = 0; y < gridMap.rows; y++)
        {
            for (int x = 0; x < gridMap.cols; x++)
            {
                GridDotData dot = gridMap.GetDotAt(x, y);
                if (dot != null)
                {
                    // Example: Store level information
                    dot.storedData = $"Level {levelNumber} - Area {x},{y}";
                    dot.customValue = levelNumber;
                    
                    // Example: Mark first row as visited
                    dot.isVisited = (y == 0);

                    // Update visual
                    Image img = dot.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = dot.isVisited ? Color.green : Color.red;
                    }

                    levelNumber++;
                }
            }
        }

        Debug.Log("Test data filled!");
    }

    /// <summary>
    /// Example: Check if a specific level is unlocked
    /// </summary>
    public bool IsLevelUnlocked(int x, int y)
    {
        GridDotData dot = gridMap.GetDotAt(x, y);
        if (dot != null)
        {
            return dot.isVisited;
        }
        return false;
    }

    /// <summary>
    /// Example: Unlock a level (mark as visited)
    /// </summary>
    public void UnlockLevel(int x, int y)
    {
        GridDotData dot = gridMap.GetDotAt(x, y);
        if (dot != null)
        {
            dot.isVisited = true;
            
            // Update visual
            Image img = dot.GetComponent<Image>();
            if (img != null)
            {
                img.color = Color.green;
            }

            Debug.Log($"Level at ({x},{y}) unlocked!");
        }
    }

    /// <summary>
    /// Example: Get level progress (how many levels completed)
    /// </summary>
    public int GetCompletedLevelsCount()
    {
        int count = 0;
        
        for (int y = 0; y < gridMap.rows; y++)
        {
            for (int x = 0; x < gridMap.cols; x++)
            {
                GridDotData dot = gridMap.GetDotAt(x, y);
                if (dot != null && dot.isVisited)
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Example: Get total number of levels
    /// </summary>
    public int GetTotalLevelsCount()
    {
        return gridMap.rows * gridMap.cols;
    }

    /// <summary>
    /// Example: Calculate completion percentage
    /// </summary>
    public float GetCompletionPercentage()
    {
        int completed = GetCompletedLevelsCount();
        int total = GetTotalLevelsCount();
        
        if (total == 0) return 0f;
        
        return (completed / (float)total) * 100f;
    }

    /// <summary>
    /// Example: Find first incomplete level
    /// </summary>
    public Vector2Int GetFirstIncompleteLevelPosition()
    {
        for (int y = 0; y < gridMap.rows; y++)
        {
            for (int x = 0; x < gridMap.cols; x++)
            {
                GridDotData dot = gridMap.GetDotAt(x, y);
                if (dot != null && !dot.isVisited)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return new Vector2Int(-1, -1); // All complete
    }

    /// <summary>
    /// Example: Unlock levels progressively (only next level can be unlocked)
    /// </summary>
    public void UnlockNextLevel()
    {
        Vector2Int nextLevel = GetFirstIncompleteLevelPosition();
        
        if (nextLevel.x >= 0 && nextLevel.y >= 0)
        {
            UnlockLevel(nextLevel.x, nextLevel.y);
        }
        else
        {
            Debug.Log("All levels completed!");
        }
    }

    // Example Unity Update method showing how to use the system
    void Update()
    {
        // Example: Press 'S' to save
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveData();
        }

        // Example: Press 'L' to load
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadData();
        }

        // Example: Press 'P' to print completion
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"Completion: {GetCompletionPercentage()}% ({GetCompletedLevelsCount()}/{GetTotalLevelsCount()})");
        }

        // Example: Press 'N' to unlock next level
        if (Input.GetKeyDown(KeyCode.N))
        {
            UnlockNextLevel();
        }
    }
}
