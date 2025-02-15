using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<GameManager>();

            return _instance;
        }
    }
    private static GameManager _instance;

    public PopupQuiz popupQuizPrefab;
    public string baseURL = "localhost";

    [HideInInspector] public WebSocketAudioClient wsAudioClient;
    [HideInInspector] public HTTPClient httpClient;

    private void Awake()
    {
        wsAudioClient = GetComponent<WebSocketAudioClient>();
        httpClient = GetComponent<HTTPClient>();
    }

    private async void Start()
    {
        await CreatePopupQuiz(Player.instance.playerQuizPopupParent);
    }

    public async Task CreatePopupQuiz(Transform parent)
    {
        QuizData quizData = await httpClient.RequestQuizData(1);

        if (quizData != null)
        {
            // Instantiate the popup
            PopupQuiz popup = Instantiate(popupQuizPrefab);
            popup.transform.parent = parent;
            popup.transform.localPosition = Vector3.zero;

            // Extract answers and correct index
            string[] answerPrompts = new string[quizData.options.Length];
            int correctIdx = -1;

            for (int i = 0; i < quizData.options.Length; i++)
            {
                answerPrompts[i] = quizData.options[i].text;
                if (quizData.options[i].isCorrect)
                {
                    correctIdx = i;
                }
            }

            // Initialize the quiz popup
            popup.Init(quizData.prompt, answerPrompts, correctIdx);
        }
    }
}
