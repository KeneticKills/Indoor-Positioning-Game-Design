using UnityEngine;
using UnityEngine.UI;
using TMPro; // If you're using TextMeshPro, otherwise use Text

/// <summary>
/// Controls the popup panel that appears when a dot is clicked
/// </summary>
public class DotPopupController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;           // The popup panel GameObject
    public TextMeshProUGUI positionText;    // Text showing position (use Text if not using TMP)
    public TextMeshProUGUI dataText;        // Text showing stored data
    public Button closeButton;              // Button to close popup
    
    [Header("Optional UI Elements")]
    public TextMeshProUGUI titleText;       // Optional: Title like "Dot Information"
    public Image dotPreview;                // Optional: Show dot color preview
    
    private static DotPopupController instance;

    void Awake()
    {
        // Singleton pattern - only one popup controller
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePopup);
        }

        // Hide popup at start
        HidePopup();
    }

    /// <summary>
    /// Show popup with dot information
    /// </summary>
    public static void ShowPopup(int x, int y, string data, GridDotData dotData = null)
    {
        if (instance == null)
        {
            Debug.LogError("DotPopupController instance not found!");
            return;
        }

        instance.DisplayPopup(x, y, data, dotData);
    }

    /// <summary>
    /// Display the popup with data
    /// </summary>
    private void DisplayPopup(int x, int y, string data, GridDotData dotData)
    {
        // Show the panel
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }

        // Update position text
        if (positionText != null)
        {
            positionText.text = $"Data saved to ({x},{y})";
        }

        // Update data text
        if (dataText != null)
        {
            if (string.IsNullOrEmpty(data))
            {
                dataText.text = "No data stored yet.";
            }
            else
            {
                dataText.text = $"Data: {data}";
            }
        }

        // Optional: Update title
        if (titleText != null)
        {
            titleText.text = $"Data Saved";
        }

        // Optional: Show dot preview color
        if (dotPreview != null && dotData != null)
        {
            Image dotImage = dotData.GetComponent<Image>();
            if (dotImage != null)
            {
                dotPreview.color = dotImage.color;
            }
        }

        Debug.Log($"Popup shown for position ({x},{y})");
    }

    /// <summary>
    /// Hide the popup
    /// </summary>
    public static void HidePopup()
    {
        if (instance == null) return;

        if (instance.popupPanel != null)
        {
            instance.popupPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Toggle popup visibility
    /// </summary>
    public static void TogglePopup()
    {
        if (instance == null) return;

        if (instance.popupPanel != null)
        {
            instance.popupPanel.SetActive(!instance.popupPanel.activeSelf);
        }
    }
}
