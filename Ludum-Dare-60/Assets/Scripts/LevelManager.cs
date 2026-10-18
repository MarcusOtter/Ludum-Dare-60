using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public UnityEvent<int> onLevelStarted = new();
    public UnityEvent<int> onLevelEnded = new();

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
        {
            return;
        }

        var sceneIndex = SimpleSceneManager.CurrentSceneIndex;
        onLevelStarted?.Invoke(sceneIndex);
    }

    public void LoadLevel(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        onLevelStarted?.Invoke(sceneIndex);
    }
}
