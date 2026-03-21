using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class StructureViewerControllerInputDetector : MonoBehaviour
{
    public GameObject buttonCanvasObject;
    private InputDevice rightController;
    private bool rightSecondaryPressed;

    void Start()
    {
        // Ensure the menu starts off so the first press opens it
        if (buttonCanvasObject != null)
        {
            buttonCanvasObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!rightController.isValid)
        {
            InitializeController();
        }
        else
        {
            HandleInput();
        }
    }

    private void InitializeController()
    {
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, inputDevices);
        if (inputDevices.Count > 0)
        {
            rightController = inputDevices[0];
        }
    }

    private void HandleInput()
    {
        // TryGetFeatureValue returns true if the feature is available, 
        // and 'isPressed' tells us the actual state of the button.
        if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressed))
        {
            if (isPressed) 
            {
                // Only trigger if this is a NEW press (wasn't pressed last frame)
                if (!rightSecondaryPressed)
                {
                    ToggleMenu();
                    rightSecondaryPressed = true; 
                }
            }
            else 
            {
                // Reset the flag only when the user actually lets go
                rightSecondaryPressed = false;
            }
        }
    }

    private void ToggleMenu()
    {
        if (buttonCanvasObject == null) return;

        StructureViewerUI uiScript = buttonCanvasObject.GetComponent<StructureViewerUI>();

        if (!buttonCanvasObject.activeSelf)
        {
            // Position and build the UI first
            uiScript.Open();
            // Then make it visible
            buttonCanvasObject.SetActive(true);
        }
        else
        {
            // Clean up the UI
            uiScript.Close();
            // Then hide it
            buttonCanvasObject.SetActive(false);
        }
    }
}