using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    Transform mainCam; 
    Transform unit; 
    Transform worldSpaceCanvas; 

    public Vector3 offset; 

    void Start()
    {
        mainCam = Camera.main.transform; 
        unit = transform.parent; 
        worldSpaceCanvas = FindObjectOfType<Canvas>().transform;

        transform.SetParent(worldSpaceCanvas);
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - mainCam.position);
        transform.position = unit.position + offset;
    }
}