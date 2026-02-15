using System.Collections;

using UnityEngine;

public class GenericStructureButton_Down : MonoBehaviour
{
    // Time that the button is inactive after release
    public float deadTime = 1.0f;

    // Reference to the GenericStructure
    public GenericStructure Structure;

    // Used to lock button during dead time
    private bool _deadTimeActive = false;

    private void Awake()
    {
        // Auto-assign if not set in Inspector
        if (Structure == null)
        {
            Structure = FindAnyObjectByType<GenericStructure>();
        }
    }

    // Trigger enter = button press
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Button") && !_deadTimeActive)
        {
            if (Structure != null)
            {
                Structure.DecreaseSize();
                Debug.Log("VR Button pressed: Grid size Decreased");
            }
            else
            {
                Debug.LogWarning("SimpleCubicStructure reference not found in scene!");
            }
        }
    }

    // Trigger exit = button release
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Button") && !_deadTimeActive)
        {
            Debug.Log("VR Button released");
            StartCoroutine(WaitForDeadTime());
        }
    }

    // Locks button activity for deadTime seconds
    private IEnumerator WaitForDeadTime()
    {
        _deadTimeActive = true;
        yield return new WaitForSeconds(deadTime);
        _deadTimeActive = false;
    }
}
