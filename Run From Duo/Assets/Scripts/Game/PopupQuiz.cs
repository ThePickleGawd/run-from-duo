using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupQuiz : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] answerTexts;
    public Button[] answerButtons;

    public UnityEvent OnCorrectAnswer;
    public UnityEvent OnWrongAnswer;

    public void Init(string question, string[] answerPrompts, int correctIdx)
    {
        questionText.text = question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            TextMeshProUGUI answerText = answerTexts[i];
            answerText.text = answerPrompts[i];

            Button b = answerButtons[i];
            if (i == correctIdx)
                b.onClick.AddListener(RightAnswer);
            else
                b.onClick.AddListener(WrongAnswer);
        }
    }

    private void WrongAnswer()
    {
        OnWrongAnswer?.Invoke();
        Destroy(gameObject);
    }

    private void RightAnswer()
    {
        OnCorrectAnswer?.Invoke();
        Destroy(gameObject);
    }
}

[System.Serializable]
public class QuizData
{
    public string prompt;
    public QuizOption[] options;
}

[System.Serializable]
public class QuizOption
{
    public string text;
    public bool isCorrect;
}