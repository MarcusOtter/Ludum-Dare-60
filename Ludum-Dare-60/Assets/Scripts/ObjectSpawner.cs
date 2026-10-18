using System.Linq;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public bool IsDone => _isDone;

    [SerializeField] private FromDirection spawnerDirection;
    [SerializeField] private int targetBucketIndex;
    [SerializeField] private float floatTime = 2f;

    private SpawnEvent[] _spawnEvents;
    
    private Bucket[] _buckets;
    private float _startTime;
    private int _nextObjectIndex;
    private bool _isDone;
    
    private void Start()
    {
        _spawnEvents = Resources
            .LoadAll<SpawnEvent>($"Levels/{gameObject.scene.buildIndex}")
            .Where(x => x.fromDirection == spawnerDirection)
            .OrderBy(x => x.time)
            .ToArray();

        _buckets = FindObjectsByType<Bucket>().OrderBy(x => x.name).ToArray();
        _startTime = Time.time;

        if (_spawnEvents == null || _spawnEvents.Length == 0)
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

        var spawnCount = _spawnEvents.Length;
        var elapsedTime = Time.time - _startTime;

        while (_nextObjectIndex < spawnCount && elapsedTime >= _spawnEvents[_nextObjectIndex].time)
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
        var recycleObject = Instantiate(_spawnEvents[index].recycleObject, transform.position, Quaternion.identity);
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
        enabled = false;
        LevelManager.Instance.CheckLevelCompleted();
    }
}
