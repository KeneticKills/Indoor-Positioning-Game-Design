using UnityEngine;

public class ScanPopupCall : MonoBehaviour
{
    public ScanPopupController popupController;

    public void OnScanClicked()
    {
        if (popupController != null)
        {
            popupController.ShowPopup();
        }
        else
        {
            Debug.LogWarning("ScanPopupCall: popupController not assigned.");
        }
    }

    public void OnScanClickedWithMessage(string message)
    {
        if (popupController != null)
            popupController.ShowPopup(message);
    }
}
