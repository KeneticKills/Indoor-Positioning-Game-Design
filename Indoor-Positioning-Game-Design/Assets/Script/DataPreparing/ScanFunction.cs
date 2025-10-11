using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System.IO;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Android;

public class ScanFunction : MonoBehaviour
{
    [SerializeField] private bool useInput;
    [SerializeField] private Transform InputX;
    [SerializeField] private Transform InputZ;

    private void Start()
    {
        if (useInput)
        {

            TMP_InputField inputXField = InputX.GetComponent<TMP_InputField>();
            TMP_InputField inputZField = InputZ.GetComponent<TMP_InputField>();

            inputXField.text = "";
            inputZField.text = "";
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
    }

    string CleanInput(string input)
    {
        return input.Replace("\u200B", ""); // remove ZERO WIDTH SPACE
    }

    public void onClick(string buttonType) {
        switch (buttonType.ToUpper()) {
            case "SCAN":
                scanFunction();
                break;
            case "RESULT":
                resultFunction();
                break;
            case "BACK":
                backFunction();
                break;
            default:
                Debug.Log(name + " used unavailable button type (" + buttonType + ")");
                break;
        }
    }

    private void backFunction()
    {
        SceneManager.LoadScene("PositionMenu");
    }

    private void resultFunction()
    {
        SceneManager.LoadScene("RSSIResultTab");
    }

    private void scanFunction() {
        TMP_InputField inputXField = InputX.GetComponent<TMP_InputField>();
        TMP_InputField inputZField = InputZ.GetComponent<TMP_InputField>();
        string inputX = CleanInput(inputXField.text.Trim());
        string inputZ = CleanInput(inputZField.text.Trim());

        inputXField.text = "";
        inputZField.text = "";

        string locationData = "Input X : " + inputX + " ; Input Z : " + inputZ;
        Debug.Log(locationData);

        if (!float.TryParse(inputX, out float xValue))
        {
            Debug.LogWarning("Invalid X value: " + inputX + " : " + xValue);
            return;
        }
        if (!float.TryParse(inputZ, out float zValue))
        {
            Debug.LogWarning("Invalid Z value: " + inputZ + " : " + zValue);
            return;
        }

        string fileName = "location_data.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        bool fileExists = File.Exists(filePath);

        if (fileExists)
        {
            bool duplicate = File.ReadLines(filePath).Any(line =>
            {
                string[] parts = line.Split(',');
                return parts.Length >= 2 && parts[0] == inputX && parts[1] == inputZ;
            });

            if (duplicate)
            {
                Debug.LogWarning($"Duplicate X,Z found: {inputX},{inputZ}");
                return;
            }
        }

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            if (!fileExists)
            {
                writer.WriteLine("x,z");
            }

            writer.WriteLine($"{inputX},{inputZ}");
        }

        Debug.Log(filePath);
        Debug.Log("Success");
    }
}
