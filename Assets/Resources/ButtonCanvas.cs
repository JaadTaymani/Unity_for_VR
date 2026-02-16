using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

// A class used for the canvas that contains the buttons to switch structure.
public class ButtonCanvas : MonoBehaviour
{
    public GenericStructure structure;
    private InputDevice rightController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Enumerates through the JSON files in the "saved structures" folder and for each of them creates a button with text from its Name field that will load that structure pressed.
        IEnumerable<string> JSONFiles = Directory.EnumerateFiles("Assets/Resources/saved structures", "*.json");
        int index = 0;
        foreach (string path in JSONFiles)
        {
            AddButton(JsonUtility.FromJson<SaveFile>(File.ReadAllText(path)).Name, index, path);
            index++;
        }

        // Rescales the canvas and places it in front of the XR origin.
        GetComponent<RectTransform>().localScale = new(0.005f, 0.005f, 0.005f);
        GetComponent<RectTransform>().position = new(0, 2, 4);
    }

    void Update()
    {
        if (!rightController.isValid)
        {
            print("Controller invalid; attempting to validate.");
            List<InputDevice> inputDevices = new();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, inputDevices);
            if (inputDevices.Count > 0)
            {
                rightController = inputDevices[0];
            }
        }
        else
        {
            print("Controller valid; attempting to read value.");
            if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButton))
            {
                print("Value read: " + secondaryButton.ToString());
            }
        }
    }

    // Called in the above foreach loop to add each button to the canvas.
    private void AddButton(string name, int index, string path)
    {
        GameObject button = DefaultControls.CreateButton(new DefaultControls.Resources());
        button.transform.SetParent(transform, false);
        button.GetComponent<RectTransform>().position += new Vector3(450, 237.5f - index * 30, 0);
        button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
        button.GetComponentInChildren<Text>().text = name;
        button.GetComponent<Button>().onClick.AddListener(() => ButtonPressed(button, path));
    }

    // Called when a button is pressed to load its corresponding structure.
    private void ButtonPressed(GameObject button, string path)
    {
        structure.LoadData(path);
    }
}
