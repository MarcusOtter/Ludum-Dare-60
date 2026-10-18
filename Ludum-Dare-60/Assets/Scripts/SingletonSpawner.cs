using UnityEngine;

public class SingletonSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] prefabsToSpawn;

    private void Awake()
    {
        foreach (var prefab in prefabsToSpawn)
        {
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }
}
