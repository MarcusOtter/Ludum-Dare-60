using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private Rigidbody2D prefabToSpawn;
    [SerializeField] private Vector2 velocity;

    private float _lastSpawnTime;
    
    private void Start()
    {
        
    }

    
    private void Update()
    {
        if (Time.time > _lastSpawnTime + 5f)
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        var rb = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
        rb.linearVelocity = velocity;
        
        _lastSpawnTime = Time.time;
    }
}
