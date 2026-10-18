using System.Linq;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    [SerializeField] private float floatTime = 2f;
    [SerializeField, Min(0f)] private float lateHitTolerance = 0.1f;

    [SerializeField] private float bucketYOffset = 1.25f;
    [SerializeField] private BoxCollider2D leftWall;
    [SerializeField] private BoxCollider2D rightWall;
    
    private Bucket[] _buckets;
    private Animator _animator;

    private static readonly int Hit = Animator.StringToHash(nameof(Hit));
    
    private void Start()
    {
        _buckets = FindObjectsByType<Bucket>().OrderBy(x => x.name).ToArray();;
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        coll.TryGetComponent<RecycleObject>(out var recycleObject);
        if (recycleObject == null)
        {
            return;
        }

        // Allow a collision that arrives slightly below the paddle's center.
        if (recycleObject.transform.position.y < transform.position.y - lateHitTolerance)
        {
            return;
        }

        if (!recycleObject.FallingDown)
        {
            return;
        }

        var direction = recycleObject.GoingRight ? 1 : -1;
        var oldIndex = recycleObject.TargetBucketIndex;
        var newIndex = Mathf.Clamp(oldIndex + direction, 0, _buckets.Length - 1);

        var hitBoundary = newIndex == oldIndex;
        var target = (Vector2) _buckets[newIndex].transform.position;

        target.y += bucketYOffset;

        // Wall bounce
        if (hitBoundary)
        {
            var wall = direction > 0 ? rightWall : leftWall;
            var wallX = direction > 0
                ? wall.bounds.min.x - recycleObject.Radius
                : wall.bounds.max.x + recycleObject.Radius;

            target.x = 2f * wallX - target.x;
        }

        AudioPlayer.Instance.PlayBounce();
        _animator.SetTrigger(Hit);
        recycleObject.LaunchTowards(target, newIndex, floatTime, recycleObject.RemainingFireBounces - 1);
    }
}
