using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

[RequireComponent(typeof(Animator))]
public class GameOverAnim : MonoBehaviour
{
    private static readonly int Start = Animator.StringToHash("Start");
    private static readonly int GameOver = Animator.StringToHash("GameOver");

    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite gameOverSprite;
    [SerializeField] private Sprite levelCompletedSprite;
    [SerializeField] private GameObject blocker;
    [SerializeField] private TMP_Text survivalTimeText;

    private Animator _animator;
    private GameObject _retryButton;
    private GameObject _nextLevelButton;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _retryButton = transform.Find("Retry").gameObject;
        _nextLevelButton = transform.Find("Next Level").gameObject;
        ClearSurvivalTime();
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
        ClearSurvivalTime();
        blocker.SetActive(false);
        _animator.SetTrigger(Start);
    }
    
    private void HandleLevelStarted(int sceneIndex)
    {
        // The infinite director will take ownership and write its timer on
        // Level 5. Every finite level starts with no survival-time text.
        ClearSurvivalTime();
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
            StartCoroutine(SetSurvivalTimeAfterLevelEnd());
            _animator.ResetTrigger(Start);
            _animator.SetTrigger(GameOver);
        }
    }

    private void HandleLevelCompleted(int sceneIndex)
    {
        ClearSurvivalTime();
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

    private void SetSurvivalTime()
    {
        if (survivalTimeText == null)
        {
            return;
        }

        if (!InfiniteSpawnDirector.HasActiveRun)
        {
            ClearSurvivalTime();
            return;
        }

        survivalTimeText.gameObject.SetActive(true);
        survivalTimeText.text = InfiniteSpawnDirector.LastSurvivalTime.ToString("0.00", CultureInfo.InvariantCulture) + "s";
    }

    private IEnumerator SetSurvivalTimeAfterLevelEnd()
    {
        // Level-end listeners are invoked in subscription order. Waiting one
        // frame lets InfiniteSpawnDirector freeze its final time first.
        yield return null;
        SetSurvivalTime();
    }

    private void ClearSurvivalTime()
    {
        if (survivalTimeText == null)
        {
            return;
        }

        survivalTimeText.text = string.Empty;
        survivalTimeText.gameObject.SetActive(false);
    }

}
