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
            print("lost 1 life");
            // -- life
        }
        
        Destroy(other.gameObject, 2f);
    }
}
