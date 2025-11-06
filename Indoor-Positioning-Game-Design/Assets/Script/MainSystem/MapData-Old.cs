using UnityEngine;
using UnityEngine.UI;

public class MapData : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public RectTransform mapArea;     // Your MapPanel (the red area)
    public GameObject dotPrefab;      // Red dot prefab (must have Image and Button)

    [Header("Grid Settings")]
    public int rows = 5;
    public int cols = 5;
    public float lineThickness = 4f;
    public Color lineColor = Color.white;

    [Header("Padding Settings")]
    [Tooltip("Base padding around the grid. Automatically adjusts depending on grid size.")]
    public float basePadding = 30f;

    private RectTransform linesParent;
    private RectTransform dotsParent;

    void Start()
    {
        GenerateGrid();
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

        // Create parents for layering
        linesParent = CreateLayer("LinesParent");
        dotsParent = CreateLayer("DotsParent");

        // Ensure lines behind, dots in front
        linesParent.SetAsFirstSibling();
        dotsParent.SetAsLastSibling();

        // Get full map size
        float fullWidth = mapArea.rect.width;
        float fullHeight = mapArea.rect.height;

        // --- AUTO PADDING ---
        // Larger grids = smaller padding, smaller grids = larger padding
        float gridFactor = Mathf.Clamp01(((rows + cols) / 2f) / 10f); // normalize roughly between 0 and 1
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
        for (int y = 0; y < rows; y++)
        {
            float posY = height / 2f - y * spacingY;
            Vector2 start = new Vector2(-width / 2f, posY);
            Vector2 end = new Vector2(width / 2f, posY);
            CreateLine(linesParent, start, end);
        }

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

                GameObject dot = Instantiate(dotPrefab, dotsParent, false);
                RectTransform drt = dot.GetComponent<RectTransform>();
                drt.anchoredPosition = new Vector2(posX, posY);
                dot.transform.SetAsLastSibling();
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
        img.raycastTarget = false; // so lines don't block dot clicks

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
}
