using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public int Hearts => _hearts;
    
    public UnityEvent onMainMenuLoaded = new();
    public UnityEvent<int> onLevelStarted = new();
    public UnityEvent<int> onLevelEnded = new();

    private static readonly int HeartDmg = Animator.StringToHash("TakeDamage");
    private static readonly int HeartDisappear = Animator.StringToHash("Disappear");
    private static readonly int HeartReset  = Animator.StringToHash("Disappear");

    [SerializeField] private GameObject heartContainer;
    [SerializeField] private Animator[] heartAnimators;

    private int _hearts;

    private void Start()
    {
        var sceneIndex = SimpleSceneManager.CurrentSceneIndex;
        ChangeScene(sceneIndex);
    }

    public void LoadLevel(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        ChangeScene(sceneIndex);
    }

    public void TakeDamage()
    {
        if (_hearts <= 0)
        {
            return;
        }

        var lastHeartIndex = _hearts - 1;
        heartAnimators[lastHeartIndex].SetTrigger(HeartDisappear);

        for (var i = 0; i < lastHeartIndex; i++)
        {
            heartAnimators[i].SetTrigger(HeartDmg);
        }

        _hearts--;

        if (_hearts <= 0)
        {
            onLevelEnded.Invoke(SimpleSceneManager.CurrentSceneIndex);
        }
    }

    private void ChangeScene(int sceneIndex)
    {
        if (sceneIndex == 0)
        {
            heartContainer.SetActive(false);
            onMainMenuLoaded.Invoke();
        }
        else
        {
            onLevelStarted?.Invoke(sceneIndex);
            _hearts = heartAnimators.Length;
            foreach (var animator in heartAnimators)
            {
                animator.ResetControllerState();
            }
            heartContainer.SetActive(true);
        }
    }

}
