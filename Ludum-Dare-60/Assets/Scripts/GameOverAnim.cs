using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class GameOverAnim : MonoBehaviour
{
    private static readonly int Start = Animator.StringToHash("Start");
    private static readonly int GameOver = Animator.StringToHash("GameOver");

    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite gameOverSprite;
    [SerializeField] private Sprite levelCompletedSprite;
    [SerializeField] private GameObject blocker;

    private Animator _animator;
    private GameObject _retryButton;
    private GameObject _nextLevelButton;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _retryButton = transform.Find("Retry").gameObject;
        _nextLevelButton = transform.Find("Next Level").gameObject;
    }

    private void OnEnable()
    {
        LevelManager.Instance.onLevelEnded.AddListener(HandleLevelEnded);
        LevelManager.Instance.onLevelCompleted.AddListener(HandleLevelCompleted);
        LevelManager.Instance.onMainMenuLoaded.AddListener(HandleMainMenuLoaded);
        LevelManager.Instance.onLevelStarted.AddListener(HandleLevelStarted);
    }

    private void OnDisable()
    {
        LevelManager.Instance.onLevelEnded.RemoveListener(HandleLevelEnded);
        LevelManager.Instance.onLevelCompleted.RemoveListener(HandleLevelCompleted);
        LevelManager.Instance.onLevelStarted.RemoveListener(HandleLevelStarted);
        LevelManager.Instance.onMainMenuLoaded.RemoveListener(HandleMainMenuLoaded);
    }
    
    private void HandleMainMenuLoaded()
    {
        blocker.SetActive(false);
        _animator.SetTrigger(Start);
    }
    
    private void HandleLevelStarted(int sceneIndex)
    {
        blocker.SetActive(false);
        _animator.SetTrigger(Start);
    }
    
    private void HandleLevelEnded(int sceneIndex)
    {
        if (LevelManager.Instance.Hearts == 0)
        {
            _retryButton.SetActive(true);
            _nextLevelButton.SetActive(false);
            blocker.SetActive(true);
            SetResultSprite(gameOverSprite);
            _animator.ResetTrigger(Start);
            _animator.SetTrigger(GameOver);
        }
    }

    private void HandleLevelCompleted(int sceneIndex)
    {
        _retryButton.SetActive(false);
        _nextLevelButton.SetActive(SimpleSceneManager.HasNextScene());
        blocker.SetActive(true);
        SetResultSprite(levelCompletedSprite);
        _animator.ResetTrigger(Start);
        _animator.SetTrigger(GameOver);
    }

    private void SetResultSprite(Sprite resultSprite)
    {
        if (resultSprite != null)
        {
            resultImage.sprite = resultSprite;
        }
    }

}
