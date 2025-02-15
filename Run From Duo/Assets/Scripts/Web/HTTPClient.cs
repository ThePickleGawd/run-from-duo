using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class HTTPClient : MonoBehaviour
{
    public async Task<QuizData> RequestQuizData(int level)
    {
        string url = $"http://{GameManager.instance.baseURL}:3000/quiz/{level}"; // Construct API URL
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();  // Wait asynchronously
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                return JsonUtility.FromJson<QuizData>(json);
            }
            else
            {
                Debug.LogError("Error fetching quiz: " + request.error);
                return null;
            }
        }
    }
}
