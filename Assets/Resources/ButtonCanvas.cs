using System.Collections.Generic;
using System.IO;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// A class used for the canvas that contains the buttons to switch structure.
public class ButtonCanvas : MonoBehaviour
{
    public Camera xrCamera;
    public GenericStructure structure;
    public string millerIndicesScene; // A path to the scene containing the miller indices which can be set in the editor.
    public string quizScene; // A path to the scene containing the quiz which can be set in the editor.
    private Vector3 localX;
    private Vector3 localY = new(0, 1, 0);
    private Vector3 localZ;
    private string currentMenu = "load";
    private GameObject millerIindicesButton;
    private GameObject quizButton;
    private GameObject loadButton;
    private GameObject editButton;
    private GameObject infoButton;
    private List<GameObject> loadObjectList = new();
    private List<GameObject> editObjectList = new();
    private List<GameObject> infoObjectList = new();
    private DefaultControls.Resources resources = new();
    private bool structureLoaded;
    private string currentStructurePath;

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

        millerIindicesButton = DefaultControls.CreateButton(resources);
        millerIindicesButton.transform.SetParent(transform, false);
        millerIindicesButton.GetComponent<RectTransform>().position += 0.225f * localX + 0.9f * localY;
        millerIindicesButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 70);
        millerIindicesButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);
        millerIindicesButton.GetComponentInChildren<Text>().text = "Miller Indices";
        millerIindicesButton.GetComponentInChildren<Text>().fontSize = 10;
        millerIindicesButton.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(millerIndicesScene));

        quizButton = DefaultControls.CreateButton(resources);
        quizButton.transform.SetParent(transform, false);
        quizButton.GetComponent<RectTransform>().position += -0.225f * localX + 0.9f * localY;
        quizButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 70);
        quizButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);
        quizButton.GetComponentInChildren<Text>().text = "Quiz";
        quizButton.GetComponentInChildren<Text>().fontSize = 10;
        quizButton.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(quizScene));

        loadButton = DefaultControls.CreateButton(resources);
        loadButton.transform.SetParent(transform, false);
        loadButton.GetComponent<RectTransform>().position += 0.85f/3 * localX + 0.75f * localY;
        loadButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 140/3);
        loadButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);
        loadButton.GetComponentInChildren<Text>().text = "Load";
        loadButton.GetComponentInChildren<Text>().fontSize = 10;
        loadButton.GetComponent<Button>().onClick.AddListener(() => SwitchMenu("load"));

        editButton = DefaultControls.CreateButton(resources);
        editButton.transform.SetParent(transform, false);
        editButton.GetComponent<RectTransform>().position += 0.75f * localY;
        editButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 140/3);
        editButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);
        editButton.GetComponentInChildren<Text>().text = "Edit";
        editButton.GetComponentInChildren<Text>().fontSize = 10;
        editButton.GetComponent<Button>().onClick.AddListener(() => SwitchMenu("edit"));

        infoButton = DefaultControls.CreateButton(resources);
        infoButton.transform.SetParent(transform, false);
        infoButton.GetComponent<RectTransform>().position += -0.85f/3 * localX + 0.75f * localY;
        infoButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 140/3);
        infoButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);
        infoButton.GetComponentInChildren<Text>().text = "Info";
        infoButton.GetComponentInChildren<Text>().fontSize = 10;
        infoButton.GetComponent<Button>().onClick.AddListener(() => SwitchMenu("info"));

        AddLoadObjectList();
        structureLoaded = false;
    }

    // Switches the menu displayed to either "load", "edit" or "info".
    private void SwitchMenu(string newMenu)
    {
        if (currentMenu != newMenu)
        {
            currentMenu = newMenu;
            if (newMenu == "load")
            { 
                AddLoadObjectList();
                DestroyObjectsFromList(ref editObjectList);
                DestroyObjectsFromList(ref infoObjectList);
            }
            if (newMenu == "edit" & structureLoaded)
            {
                DestroyObjectsFromList(ref loadObjectList);
                AddEditObjectList();
                DestroyObjectsFromList(ref infoObjectList);
            }
            if (newMenu == "info" & structureLoaded)
            {
                DestroyObjectsFromList(ref loadObjectList);
                DestroyObjectsFromList(ref editObjectList);
                AddInfoObjectList();
            }
        }
    }

    // Adds the required objects to the "load" menu.
    private void AddLoadObjectList()
    {
        // Enumerates through the JSON files in the "saved structures" folder and for each of them creates a button with text from its Name field that will load that structure pressed.
        IEnumerable<string> JSONFiles = Directory.EnumerateFiles("Assets/Resources/saved structures", "*.json");
        int index = 0;
        foreach (string path in JSONFiles)
        {
            AddLoadButton(JsonUtility.FromJson<SaveFile>(File.ReadAllText(path)).Name, index, path);
            index++;
        }
    }

    // Called in the above foreach loop to add each button to the canvas.
    private void AddLoadButton(string name, int index, string path)
    {
        GameObject button = DefaultControls.CreateButton(resources);
        button.transform.SetParent(transform, false);
        button.GetComponent<RectTransform>().position += (0.6f - 0.1f * index) * localY;
        button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 160);
        button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);
        button.GetComponentInChildren<Text>().text = name;
        button.GetComponentInChildren<Text>().fontSize = 10;
        button.GetComponent<Button>().onClick.AddListener(() => ButtonPressed(path));
        loadObjectList.Add(button);
    }

    // Called when a button from the method above is pressed to load its corresponding structure.
    private void ButtonPressed(string path)
    {
        structure.LoadData(path);
        currentStructurePath = path;
        structureLoaded = true;
    }

    // Adds the required objects to the "edit" menu.
    private void AddEditObjectList()
    {
        GameObject increaseSizeButton = DefaultControls.CreateButton(resources);
        increaseSizeButton.transform.SetParent(transform, false);
        increaseSizeButton.GetComponent<RectTransform>().position += 0.225f * localX + 0.6f * localY;
        increaseSizeButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 70);
        increaseSizeButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);
        increaseSizeButton.GetComponentInChildren<Text>().text = "Increase Size";
        increaseSizeButton.GetComponentInChildren<Text>().fontSize = 10;
        increaseSizeButton.GetComponent<Button>().onClick.AddListener(structure.IncreaseSize);
        editObjectList.Add(increaseSizeButton);

        GameObject decreaseSizeButton = DefaultControls.CreateButton(resources);
        decreaseSizeButton.transform.SetParent(transform, false);
        decreaseSizeButton.GetComponent<RectTransform>().position += -0.225f * localX + 0.6f * localY;
        decreaseSizeButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 70);
        decreaseSizeButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);
        decreaseSizeButton.GetComponentInChildren<Text>().text = "Decrease Size";
        decreaseSizeButton.GetComponentInChildren<Text>().fontSize = 10;
        decreaseSizeButton.GetComponent<Button>().onClick.AddListener(structure.DecreaseSize);
        editObjectList.Add(decreaseSizeButton);
    }

    // Adds the required objects to the "info" menu.
    private void AddInfoObjectList()
    {
        GameObject infoText = DefaultControls.CreateText(resources);
        infoText.transform.SetParent(transform, false);
        infoText.GetComponent<RectTransform>().position -= 0.15f * localY;
        infoText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 150);
        infoText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 320);
        infoText.GetComponent<Text>().text = JsonUtility.FromJson<SaveFile>(File.ReadAllText(currentStructurePath)).Info;
        infoText.GetComponent<Text>().fontSize = 10;
        infoText.GetComponent<Text>();
        infoObjectList.Add(infoText);
    }

    // Destroys all objects from a list.
    private void DestroyObjectsFromList(ref List<GameObject> list)
    {
        foreach (GameObject gameObject in list)
        {
            Destroy(gameObject);
        }
        list = new();
    }

    // Called whenever the UI panel is closed to destroy all objects from it.
    public void Close()
    {
        Destroy(millerIindicesButton);
        Destroy(quizButton);
        Destroy(loadButton);
        Destroy(editButton);
        Destroy(infoButton);
        DestroyObjectsFromList(ref loadObjectList);
        DestroyObjectsFromList(ref editObjectList);
        DestroyObjectsFromList(ref infoObjectList);
    }
}
