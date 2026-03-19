using UnityEngine;
using TMPro;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [Header("Quiz Settings")]
    public QuestionSet[] questions;
    public Transform[] answerSpawnPoints;
    public TextMeshPro questionText;
    public TextMeshPro feedbackText;
    public TextMeshPro scoreText;

    int currentQuestionIndex = -1;
    int score = 0;
    int maxQuestions = 10;

    GameObject[] spawnedAnswers = new GameObject[3];
    QuestionSet[] quizQuestions;

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

    void ShuffleQuestionsArray(QuestionSet[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rand = Random.Range(i, array.Length);
            QuestionSet temp = array[i];
            array[i] = array[rand];
            array[rand] = temp;
        }
    }

    void SelectQuizQuestions()
    {
        ShuffleQuestionsArray(questions);

        int questionCount = Mathf.Min(maxQuestions, questions.Length);

        quizQuestions = new QuestionSet[questionCount];
        for (int i = 0; i < questionCount; i++)
        {
            quizQuestions[i] = questions[i];
        }
    }

    public void NextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex >= quizQuestions.Length)
        {
            ShowFinalScore();
            return;
        }

        ClearAnswers();

        QuestionSet q = quizQuestions[currentQuestionIndex];
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

        UpdateScoreDisplay();
    }

    public void AnswerSelected(bool correct)
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = correct ? "Correct!" : "Wrong!";
            feedbackText.color = correct ? Color.green : Color.red;

            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), feedbackDuration);
        }

        if (correct)
            score++;

        UpdateScoreDisplay();
        Invoke(nameof(NextQuestion), feedbackDuration);
    }

    void HideFeedback()
    {
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
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

        questionText.text = $"Quiz Complete!\nScore: {score}/{quizQuestions.Length}\nRestarting in 5 seconds...";

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        if (scoreText != null)
            scoreText.text = $"Score: {score}/{quizQuestions.Length}";

        quizFinished = true;

        Debug.Log($"Quiz finished! Final score: {score}/{quizQuestions.Length}");

        // Auto restart after 5 seconds
        Invoke(nameof(RestartQuiz), 5f);
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}/{quizQuestions.Length}";
    }

    public void RestartQuiz()
    {
        CancelInvoke();

        currentQuestionIndex = -1;
        score = 0;
        quizFinished = false;

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        SelectQuizQuestions();
        UpdateScoreDisplay();
        NextQuestion();
    }

    public void TryRestart()
    {
        if (quizFinished)
            RestartQuiz();
    }
}