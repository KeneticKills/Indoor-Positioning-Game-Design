using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

/// <summary>
/// Attach this to each dot prefab to store its grid position and data
/// </summary>
public class GridDotData : MonoBehaviour
{
    [Header("Grid Position")]
    public int gridX;
    public int gridY;
    
    [Header("Stored Data")]
    public string storedData = "";
    public bool isVisited = false;
    public int customValue = 0;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnDotClicked);
        }
    }

    public void Initialize(int x, int y)
    {
        gridX = x;
        gridY = y;
        gameObject.name = $"Dot ({x},{y})";
    }

    private void OnDotClicked()
    {
        Debug.Log($"Clicked dot at position ({gridX}, {gridY})");
        
        // Toggle visited state
        isVisited = !isVisited;
        
        // Update visual
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.color = isVisited ? Color.green : Color.red;
        }

        // Show popup with position and data
        DotPopupController.ShowPopup(gridX, gridY, storedData, this);
    }

    public void SetData(string data)
    {
        storedData = data;
    }

    public string GetData()
    {
        return storedData;
    }
}
