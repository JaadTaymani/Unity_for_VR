using System.Collections.Generic;
using System.IO;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// A class used for the canvas that contains the buttons to change scene from the quiz scene.
public class QuizUI : MonoBehaviour
{
    public Camera xrCamera;
    public string structureViewerScene; // A path to the scene containing the structure viewer which can be set in the editor.
    public string millerIndicesScene; // A path to the scene containing the Miller indices which can be set in the editor.
    private Vector3 localX;
    private Vector3 localY = new(0, 1, 0);
    private Vector3 localZ;
    private GameObject structureViewerButton;
    private GameObject millerIndicesButton;
    private DefaultControls.Resources resources = new();

    private const int CANVAS_DISTANCE = 4; // The distance in metres of the UI panel from the user when it is placed.

    // Called whenever the UI panel is opened.
    public void Open()
    {
        GetComponent<RectTransform>().position = xrCamera.GetComponent<Transform>().position + CANVAS_DISTANCE * xrCamera.GetComponent<Transform>().forward; // Places the UI panel CANVAS_DISTANCE metres away from the user.
        GetComponent<RectTransform>().rotation = xrCamera.GetComponent<Transform>().rotation.ConstrainYaw(); // Rotates the UI panel to face the user.

        // Determines local coordinate unit vectors localX, localY and localZ to go horizontally along, vertically along and through the panel respectively.
        localZ = xrCamera.GetComponent<Transform>().forward;
        localX = new(-localZ.z, 0, localZ.x);
        localX.Normalize();

        // The rest of this code block adds the buttons to the screen.

        structureViewerButton = DefaultControls.CreateButton(resources);
        structureViewerButton.transform.SetParent(transform, false);
        structureViewerButton.GetComponent<RectTransform>().position += 0.125f * localY;
        structureViewerButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 160);
        structureViewerButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);
        structureViewerButton.GetComponentInChildren<Text>().text = "Structure Viewer";
        structureViewerButton.GetComponentInChildren<Text>().fontSize = 10;
        structureViewerButton.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(structureViewerScene));

        millerIndicesButton = DefaultControls.CreateButton(resources);
        millerIndicesButton.transform.SetParent(transform, false);
        millerIndicesButton.GetComponent<RectTransform>().position -= 0.125f * localY;
        millerIndicesButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 160);
        millerIndicesButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);
        millerIndicesButton.GetComponentInChildren<Text>().text = "Miller Indices";
        millerIndicesButton.GetComponentInChildren<Text>().fontSize = 10;
        millerIndicesButton.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(millerIndicesScene));
    }

    // Called whenever the UI panel is closed to destroy all objects from it.
    public void Close()
    {
        Destroy(structureViewerButton);
        Destroy(millerIndicesButton);
    }
}
