using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour
{
    public static T Instance { get; private set; }
	
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = GetComponent<T>();
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}