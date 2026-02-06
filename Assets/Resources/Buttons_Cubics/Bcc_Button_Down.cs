using System.Collections;

using UnityEngine;

public class Bcc_Button_Down : MonoBehaviour
{
    // Time that the button is inactive after release
    public float deadTime = 1.0f;

    // Reference to the SimpleCubicStructure
    public BCCStructure BccStructure;

    // Used to lock button during dead time
    private bool _deadTimeActive = false;

    private void Awake()
    {
        // Auto-assign if not set in Inspector
        if (BccStructure == null)
        {
            BccStructure = FindAnyObjectByType<BCCStructure>();
        }
    }

    // Trigger enter = button press
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Button") && !_deadTimeActive)
        {
            if (BccStructure != null)
            {
                BccStructure.DecreaseSize();
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

