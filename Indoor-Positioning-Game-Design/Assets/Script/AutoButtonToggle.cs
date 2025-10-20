using UnityEngine;
using UnityEngine.UI;

public class AutoButtonToggle : MonoBehaviour
{
    public Button button;
    public Text buttonText;
    public Image buttonImage;
    public Color normalColor = new Color(0.6f, 0.7f, 1f); // your default color
    public Color scanningColor = Color.red;

    private bool isScanning = false;

    void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(ToggleScan);
        UpdateButtonState();
    }

    void ToggleScan()
    {
        isScanning = !isScanning;
        UpdateButtonState();
    }

    void UpdateButtonState()
    {
        //Update text when toggle
        if (isScanning)
        {
            buttonText.text = "Scanning . . .";
            buttonImage.color = scanningColor;
        }
        else
        {
            buttonText.text = "AUTO-SCAN";
            buttonImage.color = normalColor;
        }
    }
}