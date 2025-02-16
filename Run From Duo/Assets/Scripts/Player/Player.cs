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
    [HideInInspector] public Health health;
    public Transform playerQuizPopupParent;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath.AddListener(GameManager.instance.ResetGame);

        DontDestroyOnLoad(gameObject);
    }
}
