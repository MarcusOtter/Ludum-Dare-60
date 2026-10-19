using System.Collections;
using UnityEngine;

public class Bucket : MonoBehaviour
{
    [SerializeField] private RecycleObjectType recycleObjectType;
    [SerializeField] private int objectCapacity = 5; // 0 is infinite
    [SerializeField] private float compactingCooldownMax = 10f;
    [SerializeField] private Transform compactingProgress;
    [SerializeField] private Color color;
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private SpriteRenderer[] renderers;

    private bool _isCompacting;
    private int _objectCount;
    private Animator _animator;

    private static readonly Color CompactingColor = new Color32(128, 128, 128, 255);
    private static readonly int Success = Animator.StringToHash(nameof(Success));
    private static readonly int Compacting = Animator.StringToHash(nameof(Compacting));

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        SetColors(color, color);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        other.TryGetComponent<RecycleObject>(out var recycleObject);
        if (recycleObject == null)
        {
            return;
        }
        
        if (_isCompacting || recycleObject.recycleObjectType != recycleObjectType || recycleObject.RemainingFireBounces > 0)
        {
            LevelManager.Instance.TakeDamage();
            // StartCoroutine(StartCompacting()); // Take damage = cooldown
        }
        else
        {
            AudioPlayer.Instance.PlaySuccess();
            _animator.SetTrigger(Success);
        }
        
        _objectCount++;
        if (objectCapacity > 0)
        {
            SetProgressScale((float) _objectCount / objectCapacity);
        }

        if (objectCapacity > 0 && _objectCount >= objectCapacity)
        {
            StartCoroutine(StartCompacting());
        }
        
        Destroy(other.gameObject, 2f);
    }

    private IEnumerator StartCompacting()
    {
        if (_isCompacting)
        {
            yield break;
        }

        if (objectCapacity <= 0)
        {
            yield break;
        }
        
        _isCompacting = true;
        AudioPlayer.Instance.PlayCompacting();
        _animator.SetTrigger(Compacting);
        SetColors(CompactingColor, Color.white);

        if (compactingCooldownMax > 0f)
        {
            var elapsed = 0f;
            while (elapsed < compactingCooldownMax)
            {
                SetProgressScale(1f - elapsed / compactingCooldownMax);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        _objectCount = 0;
        _isCompacting = false;
        SetProgressScale(0f);
        SetColors(color, color);
    }

    private void SetColors(Color rendererColor, Color iconColor)
    {
        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                r.color = rendererColor;
            }
        }
        
        iconRenderer.color = iconColor;
    }

    private void SetProgressScale(float normalizedProgress)
    {
        compactingProgress.localScale = compactingProgress.localScale.With(
            y: Mathf.Clamp01(normalizedProgress));
    }
}
