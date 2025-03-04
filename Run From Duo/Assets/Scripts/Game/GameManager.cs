using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public AmmoCrate ammoCratePrefab;
    public Ammo ammoPrefab;
    public Weapon weaponPrefab;
    public string baseURL = "localhost";

    [HideInInspector] public WebSocketAudioClient wsAudioClient;
    [HideInInspector] public HTTPClient httpClient;

    private void Awake()
    {
        wsAudioClient = GetComponent<WebSocketAudioClient>();
        httpClient = GetComponent<HTTPClient>();
    }


    public async Task<PopupQuiz> CreatePopupQuiz(Transform parent)
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

            return popup;
        }

        return null;
    }

    public void UpdateBaseURLFromDropdown(TMPro.TMP_Dropdown dropdown)
    {
        baseURL = dropdown.options[dropdown.value].text;
        _ = wsAudioClient.ResetConnection();
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public IEnumerator RewardPlayer(string type)
    {
        yield return null; // Need to run on main thread

        if (type == "ammo") SpawnAmmo(Player.instance.transform.position);
        if (type == "primary weapon") SpawnWeapon(Player.instance.transform.position);
        if (type == "secondary weapon") SpawnWeapon(Player.instance.transform.position);
        if (type == "item") SpawnAmmo(Player.instance.transform.position);
        else Debug.Log($"{type} not supported yet");
    }

    public void SpawnAmmoCrate(Vector3 pos)
    {
        Instantiate(ammoCratePrefab, pos, Quaternion.identity);
    }

    public void SpawnAmmo(Vector3 pos)
    {
        Instantiate(ammoPrefab, pos, Quaternion.identity);
    }

    public void SpawnWeapon(Vector3 pos)
    {
        Instantiate(weaponPrefab, pos, Quaternion.identity);
    }
}
