using UnityEngine;

public class AxisObject : MonoBehaviour
{
    [Header("Axis Settings")]
    public float axisLength = 1f;
    public float axisThickness = 0.05f; // thickness of each axis

    private GameObject xAxis;
    private GameObject yAxis;
    private GameObject zAxis;

    private void Awake()
    {
        CreateAxes();
    }

    private void CreateAxes()
    {
        // Destroy old axes if they exist
        if (xAxis != null) Destroy(xAxis);
        if (yAxis != null) Destroy(yAxis);
        if (zAxis != null) Destroy(zAxis);

        // X Axis
        xAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        xAxis.name = "X Axis";
        xAxis.transform.SetParent(transform, false);
        xAxis.transform.localScale = new Vector3(axisLength, axisThickness, axisThickness);
        xAxis.transform.localPosition = new Vector3(axisLength / 2f, 0, 0);
        xAxis.GetComponent<Renderer>().material.color = Color.red;

        // Y Axis
        yAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        yAxis.name = "Y Axis";
        yAxis.transform.SetParent(transform, false);
        yAxis.transform.localScale = new Vector3(axisThickness, axisLength, axisThickness);
        yAxis.transform.localPosition = new Vector3(0, axisLength / 2f, 0);
        yAxis.GetComponent<Renderer>().material.color = Color.green;

        // Z Axis
        zAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zAxis.name = "Z Axis";
        zAxis.transform.SetParent(transform, false);
        zAxis.transform.localScale = new Vector3(axisThickness, axisThickness, axisLength);
        zAxis.transform.localPosition = new Vector3(0, 0, axisLength / 2f);
        zAxis.GetComponent<Renderer>().material.color = Color.blue;
    }
}