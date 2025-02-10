using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindFirstObjectByType<Player>().transform.position = transform.position;
    }
}
