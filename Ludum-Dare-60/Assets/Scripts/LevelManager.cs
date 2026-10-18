using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public UnityEvent onMainMenuLoaded = new();
    public UnityEvent<int> onLevelStarted = new();
    public UnityEvent<int> onLevelEnded = new();
    
    [SerializeField] private SpriteRenderer[] heartRenderers;

    private int _hearts;

    private void Start()
    {
        var sceneIndex = SimpleSceneManager.CurrentSceneIndex;
        ChangeScene(sceneIndex);
    }

    public void LoadLevel(int sceneIndex)
    {
        ChangeScene(sceneIndex);
        SceneManager.LoadScene(sceneIndex);
    }

    public void TakeDamage()
    {
    }

    private void ChangeScene(int sceneIndex)
    {
        if (sceneIndex == 0)
        {
            onMainMenuLoaded.Invoke();
        }
        else
        {
            onLevelStarted?.Invoke(sceneIndex);
            _hearts = heartRenderers.Length;
        }
    }

}
