using UnityEngine;
using System.Collections;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private Vector3 spawnAreaMin;
    [SerializeField] private Vector3 spawnAreaMax;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnHeight = 0f;

    private void Start()
    {
        StartCoroutine(SpawnZombies());
    }

    private IEnumerator SpawnZombies()
    {
        while (true)
        {
            SpawnZombie();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnZombie()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            spawnHeight, // Keeps zombies spawning at a fixed height
            Random.Range(spawnAreaMin.z, spawnAreaMax.z)
        );

        Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        // Draws the spawn area in the Scene view for debugging
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(
            (spawnAreaMin.x + spawnAreaMax.x) / 2,
            spawnHeight,
            (spawnAreaMin.z + spawnAreaMax.z) / 2
        );
        Vector3 size = new Vector3(
            spawnAreaMax.x - spawnAreaMin.x,
            1f, // Only for visualization in Scene view
            spawnAreaMax.z - spawnAreaMin.z
        );

        Gizmos.DrawWireCube(center, size);
    }
}
