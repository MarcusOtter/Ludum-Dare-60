
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class RecycleObject : MonoBehaviour
{
    public int RemainingFireBounces => _remainingFireBounces;
    public RecycleObjectType recycleObjectType;
    
    public int TargetBucketIndex { get; private set; }
    public bool GoingRight => _rigidbody.linearVelocity.x > 0;
    public bool FallingDown => _rigidbody.linearVelocity.y < 0;
    public float Radius => _circleCollider.radius;
    
    [SerializeField] private float timeUntilColliderEnable = 1f;
    [SerializeField] private float spinSpeed = 500f;
    [SerializeField] private GameObject fireObject;

    private Rigidbody2D _rigidbody;
    private CircleCollider2D _circleCollider;
    private SpriteRenderer _spriteRenderer;
    
    private int _remainingFireBounces;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _circleCollider = GetComponent<CircleCollider2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _circleCollider.enabled = false;
        timeUntilColliderEnable = Time.time + timeUntilColliderEnable;
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CheckLevelCompleted(this);
        }
    }

    private void Update()
    {
        _spriteRenderer.transform.Rotate(GoingRight ? Vector3.back : Vector3.forward, spinSpeed * Time.deltaTime);
        
        fireObject.SetActive(_remainingFireBounces > 0);

        if (_circleCollider.enabled)
        {
            return;
        }

        if (Time.time > timeUntilColliderEnable)
        {
            _circleCollider.enabled = true;
        }
    }
    
    public void LaunchTowards(Vector2 end, int bucketIndex, float duration, int fireBounces)
    {
        TargetBucketIndex = bucketIndex;
        var gravity = Physics2D.gravity.y * _rigidbody.gravityScale;
        var velocity = CalculateVelocity(_rigidbody.position, end, duration, gravity);
        _remainingFireBounces = Mathf.Max(fireBounces, 0);
        _rigidbody.linearVelocity = velocity;
    }

    private Vector2 CalculateVelocity(Vector2 start, Vector2 end, float time, float gravity)
    {
        var displacement = end - start;
        return new Vector2(displacement.x / time, displacement.y / time - 0.5f * gravity * time);
    }
}
