
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class RecycleObject : MonoBehaviour
{
    public int TargetBucketIndex { get; private set; }
    public bool GoingRight => _rigidbody.linearVelocity.x > 0;
    public bool FallingDown => _rigidbody.linearVelocity.y < 0;
    public float Radius => _circleCollider.radius;

    [SerializeField] private float timeUntilColliderEnable = 1f;

    private Rigidbody2D _rigidbody;
    private CircleCollider2D _circleCollider;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _circleCollider = GetComponent<CircleCollider2D>();
        _circleCollider.enabled = false;
        timeUntilColliderEnable = Time.time + timeUntilColliderEnable;
    }

    private void Update()
    {
        if (_circleCollider.enabled)
        {
            return;
        }

        if (Time.time > timeUntilColliderEnable)
        {
            _circleCollider.enabled = true;
        }
    }
    
    public void LaunchTowards(Vector2 end, int bucketIndex, float duration)
    {
        TargetBucketIndex = bucketIndex;
        var gravity = Physics2D.gravity.y * _rigidbody.gravityScale;
        _rigidbody.linearVelocity = CalculateVelocity(_rigidbody.position, end, duration, gravity);
    }

    private Vector2 CalculateVelocity(Vector2 start, Vector2 end, float time, float gravity)
    {
        var displacement = end - start;
        return new Vector2(displacement.x / time, displacement.y / time - 0.5f * gravity * time);
    }
}
