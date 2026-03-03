using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AnswerObject : MonoBehaviour
{
    public bool isCorrect;

    void Awake()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable =
            GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        interactable.selectEntered.AddListener(OnSelected);
    }

    void OnSelected(SelectEnterEventArgs args)
    {
        QuizManager.Instance.AnswerSelected(isCorrect);
    }

    public void Initialize(bool correct)
    {
        isCorrect = correct;
    }
}