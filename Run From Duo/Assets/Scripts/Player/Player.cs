using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance
    {
        get
        {
            if (playerInstance == null)
                playerInstance = FindFirstObjectByType<Player>();

            return playerInstance;
        }
    }
    private static Player playerInstance;
    public Transform playerQuizPopupParent;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
