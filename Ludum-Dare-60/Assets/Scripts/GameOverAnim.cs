using UnityEngine;

[RequireComponent(typeof(Animator))]
public class GameOverAnim : MonoBehaviour
{
    private static readonly int Start = Animator.StringToHash("Start");
    private static readonly int GameOver = Animator.StringToHash("GameOver");

    private Animator _animator;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        LevelManager.Instance.onLevelEnded.AddListener(HandleLevelEnded);
        LevelManager.Instance.onMainMenuLoaded.AddListener(HandleMainMenuLoaded);
        LevelManager.Instance.onLevelStarted.AddListener(HandleLevelStarted);
    }

    private void OnDisable()
    {
        LevelManager.Instance.onLevelEnded.RemoveListener(HandleLevelEnded);
        LevelManager.Instance.onLevelStarted.RemoveListener(HandleLevelStarted);
        LevelManager.Instance.onMainMenuLoaded.RemoveListener(HandleMainMenuLoaded);
    }
    
    private void HandleMainMenuLoaded()
    {
        _animator.SetTrigger(Start);
    }
    
    private void HandleLevelStarted(int sceneIndex)
    {
        _animator.SetTrigger(Start);
    }
    
    private void HandleLevelEnded(int sceneIndex)
    {
        if (LevelManager.Instance.Hearts == 0)
        {
            _animator.ResetTrigger(Start);
            _animator.SetTrigger(GameOver);
        }
    }
    
}
