using UnityEngine;
using TMPro;
using System.Collections;

public class ScanPopupController : MonoBehaviour
{
    [Header("Popup Elements")]
    public TMP_Text popupText;              // Main message (e.g. "Scanning completed")
    public TMP_Text popupCountdownText;     // Countdown text (e.g. "Automatically closed in . . 3")

    [Header("Popup Settings")]
    [Tooltip("How many seconds the popup stays visible before closing automatically.")]
    public int popupDuration = 3;

    private Coroutine countdownCoroutine;

    private void Start()
    {
        // Ensure popup is hidden on scene start
        gameObject.SetActive(false);
    }

    // Public method: can be called with or without a message string
    // defaultMessage used when caller doesn't provide one
    public void ShowPopup(string message = "Scanning completed")
    {
        // Stop any running coroutine to avoid duplicates
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        // Set text and enable the popup GameObject
        if (popupText != null) popupText.text = message;
        gameObject.SetActive(true);

        // Start countdown coroutine
        countdownCoroutine = StartCoroutine(AutoCloseCountdown());
    }

    private IEnumerator AutoCloseCountdown()
    {
        int timeLeft = Mathf.Max(1, popupDuration);

        while (timeLeft > 0)
        {
            if (popupCountdownText != null)
                popupCountdownText.text = $"Automatically closed in . . {timeLeft}";

            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        ClosePopup();
    }

    public void ClosePopup()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // Tap anywhere to close while popup is active
        if (gameObject.activeSelf && (Input.touchCount > 0 || Input.GetMouseButtonDown(0)))
        {
            ClosePopup();
        }
    }
}