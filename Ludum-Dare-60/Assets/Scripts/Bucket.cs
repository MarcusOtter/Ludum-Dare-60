using UnityEngine;

public class Bucket : MonoBehaviour
{
    [SerializeField] private RecycleObjectType recycleObjectType;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        other.TryGetComponent<RecycleObject>(out var recycleObject);
        if (recycleObject == null)
        {
            return;
        }

        if (recycleObject.recycleObjectType != recycleObjectType)
        {
            LevelManager.Instance.TakeDamage();
        }
        
        Destroy(other.gameObject, 2f);
    }
}
