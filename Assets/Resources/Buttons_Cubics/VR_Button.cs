using System.Collections;

using UnityEngine;

public class VRButton_Up : MonoBehaviour
{
    // Time that the button is inactive after release
    public float deadTime = 1.0f;

    // Reference to the SimpleCubicStructure
    public SimpleCubicStructure scStructure;

    // Used to lock button during dead time
    private bool _deadTimeActive = false;

    private void Awake()
    {
        // Auto-assign if not set in Inspector
        if (scStructure == null)
        {
            scStructure = FindAnyObjectByType<SimpleCubicStructure>();
        }
    }

    // Trigger enter = button press
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Button") && !_deadTimeActive)
        {
            if (scStructure != null)
            {
                scStructure.IncreaseSize();
                Debug.Log("VR Button pressed: Grid size increased");
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
