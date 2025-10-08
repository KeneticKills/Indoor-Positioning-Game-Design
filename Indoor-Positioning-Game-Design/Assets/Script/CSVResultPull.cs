using TMPro;
using UnityEngine;

public class CSVResultPull : MonoBehaviour
{
    public TextMeshProUGUI xPosition;
    public TextMeshProUGUI zPosition;

    public void SetValues(string xValue, string zValue)
    {
        xPosition.text = "X: " + xValue;
        zPosition.text = "Z: " + zValue;
    }
}
