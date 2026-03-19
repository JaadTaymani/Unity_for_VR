using UnityEngine;

[CreateAssetMenu(menuName = "Quiz/Question Set")]
public class QuestionSet : ScriptableObject
{
    [TextArea(2, 5)]
    public string questionText;

    public GameObject[] answerPrefabs = new GameObject[3];

    [Range(0, 2)]
    public int correctIndex;
}
