using UnityEngine;

public class SpinAndFloat : MonoBehaviour
{
    [Header("Spin Settings")]
    public float spinSpeed = 50f;          // degrees per second

    [Header("Float Settings")]
    public float floatAmplitude = 0.3f;    // vertical movement range
    public float floatFrequency = 1f;      // speed of floating

    private Vector3 startPos;

    void Start()
    {
        // Save the starting position for floating
        startPos = transform.position;
    }

    void Update()
    {
        // Rotate around its own pivot
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);

        // Float up and down independently of rotation
        Vector3 pos = startPos;
        pos.y += Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = pos;
    }
}
