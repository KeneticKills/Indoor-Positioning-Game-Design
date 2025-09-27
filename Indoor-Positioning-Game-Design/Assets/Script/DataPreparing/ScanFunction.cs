using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ScanFunction : MonoBehaviour
{
    public Transform InputX;
    public Transform InputZ;

    public void onClick() {
        string inputX = InputX.GetComponent<TextMeshProUGUI>().text;
        string inputZ = InputZ.GetComponent<TextMeshProUGUI>().text;
        Debug.Log("Input X : " + inputX + " ; Input Z : " + inputZ);
    }
}
