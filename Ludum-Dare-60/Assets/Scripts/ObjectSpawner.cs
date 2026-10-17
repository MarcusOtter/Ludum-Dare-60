using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private int targetBucketIndex;
    [SerializeField] private float floatTime = 3f;
    
    [Header("Level")]
    [SerializeField] private RecycleObject[] objectsToSpawn;
    [SerializeField] private float[] spawnTimes;
    [SerializeField] private UnityEvent isDone;
    
    private Bucket[] _buckets;
    private float _startTime;
    private int _nextObjectIndex;
    private bool _isDone;
    
    private void Start()
    {
        _buckets = FindObjectsByType<Bucket>().OrderBy(x => x.name).ToArray();
        _startTime = Time.time;

        if (objectsToSpawn == null || spawnTimes == null || objectsToSpawn.Length == 0 || spawnTimes.Length == 0)
        {
            Complete();
        }
    }

    private void Update()
    {
        if (_isDone)
        {
            return;
        }

        var spawnCount = Mathf.Min(objectsToSpawn.Length, spawnTimes.Length);
        var elapsedTime = Time.time - _startTime;

        while (_nextObjectIndex < spawnCount && elapsedTime >= spawnTimes[_nextObjectIndex])
        {
            Spawn(_nextObjectIndex);
            _nextObjectIndex++;
        }

        if (_nextObjectIndex >= spawnCount)
        {
            Complete();
        }
    }

    private void Spawn(int index)
    {
        var recycleObject = Instantiate(objectsToSpawn[index], transform.position, Quaternion.identity);
        var targetBucket = _buckets[targetBucketIndex].transform;
        recycleObject.LaunchTowards(targetBucket.position, targetBucketIndex, floatTime);
    }

    private void Complete()
    {
        if (_isDone)
        {
            return;
        }

        _isDone = true;
        isDone?.Invoke();
        enabled = false;
    }
}
