using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapDataWithStorage : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public RectTransform mapArea;     // Your MapPanel (the red area)
    public GameObject dotPrefab;      // Red dot prefab (must have Image, Button, and GridDotData)

    [Header("Grid Settings")]
    public int rows = 5;
    public int cols = 5;
    public List<Vector2> dotIgnore = new List<Vector2>();
    public int floor = 1;

    [Header("Grid Size (Rectangle)")]
    [Tooltip("If enabled, grid will use custom width/height instead of filling the mapArea")]
    public bool useCustomSize = false;
    public float gridWidth = 400f;    // Custom width of the grid in pixels
    public float gridHeight = 400f;   // Custom height of the grid in pixels

    [Header("Line Settings")]
    public float lineThickness = 4f;
    public Color lineColor = Color.white;

    [Header("Padding Settings")]
    [Tooltip("Base padding around the grid. Automatically adjusts depending on grid size.")]
    public float basePadding = 30f;

    [Header("Data Management")]
    public GridDataManager dataManager;

    private RectTransform linesParent;
    private RectTransform dotsParent;
    private GridDotData[,] dotDataArray; // 2D array to store all dot references

    void Start()
    {
        // Ensure data manager exists
        if (dataManager == null)
        {
            dataManager = gameObject.AddComponent<GridDataManager>();
        }

        GenerateGrid();

        // Try to load existing data
        if (dataManager.SaveFileExists())
        {
            LoadGridData();
        }
    }

    public void GenerateGrid()
    {
        if (mapArea == null || dotPrefab == null)
        {
            Debug.LogError("MapData: mapArea or dotPrefab is not assigned.");
            return;
        }

        // Clean up old grid
        for (int i = mapArea.childCount - 1; i >= 0; i--)
            DestroyImmediate(mapArea.GetChild(i).gameObject);

        // Initialize 2D array for dot data storage
        dotDataArray = new GridDotData[rows, cols];

        // Create parents for layering
        linesParent = CreateLayer("LinesParent");
        dotsParent = CreateLayer("DotsParent");

        // Ensure lines behind, dots in front
        linesParent.SetAsFirstSibling();
        dotsParent.SetAsLastSibling();

        // Determine grid dimensions based on useCustomSize setting
        float fullWidth, fullHeight;

        if (useCustomSize)
        {
            // Use custom specified size
            fullWidth = gridWidth;
            fullHeight = gridHeight;
            Debug.Log($"Using custom grid size: {gridWidth} x {gridHeight}");
        }
        else
        {
            // Use mapArea size (original behavior)
            fullWidth = mapArea.rect.width;
            fullHeight = mapArea.rect.height;
            Debug.Log($"Using mapArea size: {fullWidth} x {fullHeight}");
        }

        // --- AUTO PADDING ---
        float gridFactor = Mathf.Clamp01(((rows + cols) / 2f) / 10f);
        float dynamicPadding = Mathf.Lerp(basePadding * 1.5f, basePadding * 0.5f, gridFactor);

        // Calculate usable area (inside padding)
        float width = fullWidth - (dynamicPadding * 2f);
        float height = fullHeight - (dynamicPadding * 2f);

        // Handle edge cases
        if (cols < 2 || rows < 2)
        {
            Debug.LogWarning("MapData: rows/cols should be >= 2 for proper grid display.");
        }

        // Calculate spacing based on usable area
        float spacingX = (cols > 1) ? width / (cols - 1) : 0;
        float spacingY = (rows > 1) ? height / (rows - 1) : 0;

        // --- DRAW GRID LINES ---
        // Horizontal lines
        for (int y = 0; y < rows; y++)
        {
            float posY = height / 2f - y * spacingY;
            Vector2 start = new Vector2(-width / 2f, posY);
            Vector2 end = new Vector2(width / 2f, posY);
            CreateLine(linesParent, start, end);
        }

        // Vertical lines
        for (int x = 0; x < cols; x++)
        {
            float posX = -width / 2f + x * spacingX;
            Vector2 start = new Vector2(posX, height / 2f);
            Vector2 end = new Vector2(posX, -height / 2f);
            CreateLine(linesParent, start, end);
        }

        // --- CREATE DOTS ---
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float posX = -width / 2f + x * spacingX;
                float posY = height / 2f - y * spacingY;

                if (dotIgnore.Contains(new Vector2(x,y))) {
                    Debug.Log("Ignore X : " + x + " ; Y : " + y);
                    continue;
                }

                GameObject dot = Instantiate(dotPrefab, dotsParent, false);
                RectTransform drt = dot.GetComponent<RectTransform>();
                drt.anchoredPosition = new Vector2(posX, posY);
                dot.transform.SetAsLastSibling();

                // Initialize GridDotData component
                GridDotData dotData = dot.GetComponent<GridDotData>();
                if (dotData == null)
                {
                    dotData = dot.AddComponent<GridDotData>();
                }
                dotData.Initialize(x, y, floor);

                // Store in 2D array for easy access
                dotDataArray[y, x] = dotData;
            }
        }
    }

    private RectTransform CreateLayer(string name)
    {
        GameObject layer = new GameObject(name, typeof(RectTransform));
        layer.transform.SetParent(mapArea, false);
        RectTransform rt = layer.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        return rt;
    }

    private void CreateLine(RectTransform parent, Vector2 start, Vector2 end)
    {
        GameObject lineGO = new GameObject("Line", typeof(RectTransform), typeof(Image));
        lineGO.transform.SetParent(parent, false);

        Image img = lineGO.GetComponent<Image>();
        img.color = lineColor;
        img.raycastTarget = false;

        RectTransform rt = lineGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 dir = end - start;
        float len = dir.magnitude;
        rt.sizeDelta = new Vector2(len, lineThickness);
        rt.anchoredPosition = (start + end) / 2f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0, 0, angle);
    }

    // --- DATA MANAGEMENT METHODS ---
    // All crucial data management features preserved below:

    /// <summary>
    /// Save current grid state to CSV
    /// </summary>
    public void SaveGridData()
    {
        if (dataManager != null && dotDataArray != null)
        {
            dataManager.SaveGridDataToCSV(dotDataArray);
        }
    }

    /// <summary>
    /// Load grid state from CSV
    /// </summary>
    public void LoadGridData()
    {
        if (dataManager != null && dotDataArray != null)
        {
            dataManager.LoadGridDataFromCSV(dotDataArray);
        }
    }

    /// <summary>
    /// Get dot data at specific grid position
    /// </summary>
    public GridDotData GetDotAt(int x, int y)
    {
        if (dotDataArray != null && y >= 0 && y < rows && x >= 0 && x < cols)
        {
            return dotDataArray[y, x];
        }
        return null;
    }

    /// <summary>
    /// Set data for a specific dot
    /// </summary>
    public void SetDotData(int x, int y, string data)
    {
        GridDotData dot = GetDotAt(x, y);
        if (dot != null)
        {
            dot.SetData(data);
        }
    }

    // Example: Save on application quit
    void OnApplicationQuit()
    {
        SaveGridData();
    }
}