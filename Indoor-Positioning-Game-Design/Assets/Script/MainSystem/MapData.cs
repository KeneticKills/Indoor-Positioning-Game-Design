using UnityEngine;
using UnityEngine.UI;

public class MapData : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public RectTransform mapArea;     // your MapPanel
    public GameObject dotPrefab;      // red dot prefab (must have Image and Button)

    [Header("Grid Settings")]
    public int rows = 5;
    public int cols = 5;
    public float lineThickness = 4f;
    public Color lineColor = Color.white;

    // Parents to control render order
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
            Debug.LogError("GridGenerator_Fixed: mapArea or dotPrefab is not assigned.");
            return;
        }

        // Clean up old children safely
        for (int i = mapArea.childCount - 1; i >= 0; i--)
            DestroyImmediate(mapArea.GetChild(i).gameObject);

        // Create two parents: one for lines (back), one for dots (front)
        GameObject lp = new GameObject("LinesParent", typeof(RectTransform));
        lp.transform.SetParent(mapArea, false);
        linesParent = lp.GetComponent<RectTransform>();
        linesParent.anchorMin = linesParent.anchorMax = new Vector2(0.5f, 0.5f);
        linesParent.pivot = new Vector2(0.5f, 0.5f);
        linesParent.anchoredPosition = Vector2.zero;
        linesParent.sizeDelta = Vector2.zero;

        GameObject dp = new GameObject("DotsParent", typeof(RectTransform));
        dp.transform.SetParent(mapArea, false);
        dotsParent = dp.GetComponent<RectTransform>();
        dotsParent.anchorMin = dotsParent.anchorMax = new Vector2(0.5f, 0.5f);
        dotsParent.pivot = new Vector2(0.5f, 0.5f);
        dotsParent.anchoredPosition = Vector2.zero;
        dotsParent.sizeDelta = Vector2.zero;

        // Ensure correct sibling order: lines behind, dots front
        linesParent.SetAsFirstSibling();
        dotsParent.SetAsLastSibling();

        // Get map size (must have proper size; mapArea pivot = 0.5,0.5 recommended)
        float width = mapArea.rect.width;
        float height = mapArea.rect.height;

        if (cols < 2 || rows < 2)
        {
            Debug.LogWarning("GridGenerator_Fixed: rows/cols should be >= 2 for lines to draw nicely.");
        }

        float spacingX = (cols > 1) ? width / (cols - 1) : 0;
        float spacingY = (rows > 1) ? height / (rows - 1) : 0;

        // Draw horizontal lines (in linesParent)
        for (int y = 0; y < rows; y++)
        {
            Vector2 start = new Vector2(-width / 2f, height / 2f - y * spacingY);
            Vector2 end = new Vector2(width / 2f, height / 2f - y * spacingY);
            CreateLine(linesParent, start, end);
        }

        // Draw vertical lines (in linesParent)
        for (int x = 0; x < cols; x++)
        {
            Vector2 start = new Vector2(-width / 2f + x * spacingX, height / 2f);
            Vector2 end = new Vector2(-width / 2f + x * spacingX, -height / 2f);
            CreateLine(linesParent, start, end);
        }

        // Create dots (in dotsParent) AFTER lines so they render above
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float posX = -width / 2f + x * spacingX;
                float posY = height / 2f - y * spacingY;

                GameObject dot = Instantiate(dotPrefab, dotsParent, false);
                RectTransform drt = dot.GetComponent<RectTransform>();
                drt.anchoredPosition = new Vector2(posX, posY);

                // Make sure dot is on top within dotsParent
                dot.transform.SetAsLastSibling();

                // IMPORTANT: Ensure the dot's Image has raycastTarget = true and
                // the lines' Images (created below) have raycastTarget = false so the button receives clicks.
            }
        }
    }

    private void CreateLine(RectTransform parent, Vector2 start, Vector2 end)
    {
        GameObject lineGO = new GameObject("Line", typeof(RectTransform), typeof(Image));
        lineGO.transform.SetParent(parent, false);

        Image img = lineGO.GetComponent<Image>();
        img.color = lineColor;
        img.raycastTarget = false; // lines shouldn't block button clicks

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
