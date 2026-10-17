using System.Linq;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private int targetBucketIndex;
    [SerializeField] private RecycleObject prefabToSpawn;
    [SerializeField] private float floatTime = 3f;

    private Bucket[] _buckets;
    private float _lastSpawnTime;
    
    private void Start()
    {
        _buckets = FindObjectsByType<Bucket>().OrderBy(x => x.name).ToArray();
    }

    private void Update()
    {
        if (Time.time > _lastSpawnTime + 3f)
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        var recycleObject = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
        var targetBucket = _buckets[targetBucketIndex].transform;
        recycleObject.LaunchTowards(targetBucket.position, targetBucketIndex, floatTime);
        
        _lastSpawnTime = Time.time;
    }
}
