using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class QuizControllerInputDetector : MonoBehaviour
{
    public GameObject buttonCanvasObject;
    private InputDevice rightController;
    private bool rightSecondaryPressed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //buttonCanvasObject.SetActive(false);
        buttonCanvasObject.GetComponent<QuizUI>().Open();
    }

    // Update is called once per frame
    void Update()
    {
        // Checks if the right headset controller is connected.
        if (!rightController.isValid)
        {
            // Attempts to find the right headset controller.
            List<InputDevice> inputDevices = new();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, inputDevices);
            if (inputDevices.Count > 0)
            {
                rightController = inputDevices[0];
            }
        }
        else
        {
            // Reads the Boolean value of the right secondary button.
            if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButton))
            {
                // Calls a method if it was just pressed.
                if (!rightSecondaryPressed)
                {
                    // Opens the menu if it was closed.
                    if (!buttonCanvasObject.activeSelf)
                    {
                        buttonCanvasObject.GetComponent<MillerIndicesUI>().Open();
                        buttonCanvasObject.SetActive(true);
                    }
                    // Closes the menu if it was open.
                    else
                    {
                        buttonCanvasObject.GetComponent<MillerIndicesUI>().Close();
                        buttonCanvasObject.SetActive(false);
                    }
                }
                rightSecondaryPressed = true; // Sets a variable to true so that on the next update, if the button is still pressed, the method will not run.
            }
            else
            {
                rightSecondaryPressed = false; // Sets the variable to false so that on the next update, if the button becomes pressed, the method will run.
            }
        }
    }
}
