using UnityEngine;
using TMPro;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [Header("Quiz Settings")]
    public QuestionSet[] questions;           // All questions
    public Transform[] answerSpawnPoints;     // Spawn positions
    public TextMeshPro questionText;          // Text on wall
    public TextMeshPro feedbackText;          // Feedback text

    int currentQuestionIndex = -1;
    int score = 0;

    GameObject[] spawnedAnswers = new GameObject[3];

    bool quizFinished = false;
    float feedbackDuration = 1.2f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RestartQuiz();
    }

    void Update()
    {
        // Check for click-anywhere restart after quiz is finished
        if (quizFinished)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0))
                RestartQuiz();
#endif

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                RestartQuiz();
#endif
        }
    }

    // Shuffle the questions array
    void ShuffleQuestions()
    {
        for (int i = 0; i < questions.Length; i++)
        {
            int rand = Random.Range(i, questions.Length);
            QuestionSet temp = questions[i];
            questions[i] = questions[rand];
            questions[rand] = temp;
        }
    }

    public void NextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex >= questions.Length)
        {
            ShowFinalScore();
            return;
        }

        ClearAnswers();

        QuestionSet q = questions[currentQuestionIndex];
        questionText.text = q.questionText;

        for (int i = 0; i < 3; i++)
        {
            GameObject obj = Instantiate(
                q.answerPrefabs[i],
                answerSpawnPoints[i].position,
                answerSpawnPoints[i].rotation
            );

            AnswerObject answer = obj.GetComponent<AnswerObject>();
            answer.isCorrect = (i == q.correctIndex);

            spawnedAnswers[i] = obj;
        }
    }

    public void AnswerSelected(bool correct)
    {
        // Show feedback
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = correct ? "Correct!" : "Wrong!";
            feedbackText.color = correct ? Color.green : Color.red;
        }

        if (correct)
            score++;

        // Wait then go to next question
        Invoke(nameof(NextQuestion), feedbackDuration);
    }

    void ClearAnswers()
    {
        foreach (GameObject obj in spawnedAnswers)
        {
            if (obj != null)
                Destroy(obj);
        }
    }

    void ShowFinalScore()
    {
        ClearAnswers();
        questionText.text = $"Quiz Complete!\nScore: {score}/{questions.Length}\nClick anywhere to restart";
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        quizFinished = true;
        Debug.Log($"Quiz finished! Final score: {score}/{questions.Length}");
    }

    public void RestartQuiz()
    {
        CancelInvoke();

        currentQuestionIndex = -1;
        score = 0;
        quizFinished = false;

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        ShuffleQuestions();
        NextQuestion();
    }

    // For XR ray / controller click restart
    public void TryRestart()
    {
        if (quizFinished)
            RestartQuiz();
    }
}
