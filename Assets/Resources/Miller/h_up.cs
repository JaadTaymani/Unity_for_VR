using System.Collections;

using UnityEngine;

public class H_Up : MonoBehaviour
{
    // Time that the button is inactive after release
    public float deadTime = 1.0f;

    // Reference to the SimpleCubicStructure
    public MillerIndexGenerator Miller;

    // Used to lock button during dead time
    private bool _deadTimeActive = false;

    private void Awake()
    {
        // Auto-assign if not set in Inspector
        if (Miller == null)
        {
            Miller = FindAnyObjectByType<MillerIndexGenerator>();
        }
    }

    // Trigger enter = button press
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Button") && !_deadTimeActive)
        {
            if (Miller != null)
            {
                Miller.IncreaseSizeH();
                Debug.Log("VR Button pressed: Grid size Increased");
            }
            else
            {
                Debug.LogWarning("MillerIndexGenerator reference not found in scene!");
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
