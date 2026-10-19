using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelManager : Singleton<LevelManager>
{
    public int Hearts => _hearts;
    
    public UnityEvent onMainMenuLoaded = new();
    public UnityEvent<int> onLevelStarted = new();
    public UnityEvent<int> onLevelEnded = new();
    public UnityEvent<int> onLevelCompleted = new();

    private static readonly int HeartDmg = Animator.StringToHash("TakeDamage");
    private static readonly int HeartDisappear = Animator.StringToHash("Disappear");

    [SerializeField] private GameObject heartContainer;
    [SerializeField] private Animator[] heartAnimators;
    [SerializeField] private Image levelProgressImage;
    [FormerlySerializedAs("levelProgressParent")] [SerializeField] private GameObject levelProgressText;

    private int _hearts;
    private float _levelStartTime;
    private float _levelDuration;
    private bool _lostLifeThisLevel;

    private void Start()
    {
        var sceneIndex = SimpleSceneManager.CurrentSceneIndex;
        ChangeScene(sceneIndex);
    }

    private void Update()
    {
        if (_levelDuration <= 0f)
        {
            return;
        }

        var elapsedTime = Time.time - _levelStartTime;
        levelProgressImage.fillAmount = Mathf.Clamp01(elapsedTime / _levelDuration);
    }

    public void LoadLevel(int sceneIndex)
    {
        AudioPlayer.Instance?.StopAllSoundEffects();
        SceneManager.LoadScene(sceneIndex);
        ChangeScene(sceneIndex);
    }

    public void TakeDamage()
    {
        if (_hearts <= 0)
        {
            return;
        }

        AudioPlayer.Instance.PlayDamage();
        Camera.main.TryGetComponent<CrtFix>(out var cameraEffects);
        cameraEffects?.ShakeOnDamage();

        var lastHeartIndex = _hearts - 1;
        heartAnimators[lastHeartIndex].SetTrigger(HeartDisappear);

        for (var i = 0; i < lastHeartIndex; i++)
        {
            heartAnimators[i].SetTrigger(HeartDmg);
        }

        _hearts--;
        _lostLifeThisLevel = true;

        if (_hearts <= 0)
        {
            onLevelEnded.Invoke(SimpleSceneManager.CurrentSceneIndex);
        }
    }

    public void ConfigureInfiniteMode()
    {
        _levelDuration = 0f;
        levelProgressImage.fillAmount = 0f;
        levelProgressImage.transform.parent.gameObject.SetActive(false);
        levelProgressText.SetActive(false);
    }

    public void CheckLevelCompleted(RecycleObject removedObject = null)
    {
        if (SimpleSceneManager.CurrentSceneIndex == 0 || _hearts <= 0)
        {
            return;
        }

        var spawners = FindObjectsByType<ObjectSpawner>();
        if (spawners.Length == 0)
        {
            return;
        }

        foreach (var spawner in spawners)
        {
            if (!spawner.IsDone)
            {
                return;
            }
        }

        foreach (var recycleObject in FindObjectsByType<RecycleObject>())
        {
            if (!ReferenceEquals(recycleObject, removedObject))
            {
                return;
            }
        }

        var sceneIndex = SimpleSceneManager.CurrentSceneIndex;
        if (!_lostLifeThisLevel)
        {
            PlayerProgress.RecordFlawlessCompletion(sceneIndex);
        }

        onLevelCompleted.Invoke(sceneIndex);
    }
    
    private void ChangeScene(int sceneIndex)
    {
        var isInfinite = FindAnyObjectByType<InfiniteSpawnDirector>() != null;
        if (levelProgressImage != null)
        {
            levelProgressImage.fillAmount = 0f;
        }

        
        levelProgressImage.transform.parent.gameObject.SetActive(sceneIndex != 0 && !isInfinite);
        levelProgressText.SetActive(sceneIndex != 0 && !isInfinite);
        heartContainer.SetActive(sceneIndex != 0);
        

        if (sceneIndex == 0)
        {
            _levelDuration = 0f;
            onMainMenuLoaded.Invoke();
        }
        else
        {
            _levelStartTime = Time.time;
            _levelDuration = isInfinite ? 0f : GetLevelDuration(sceneIndex);
            _lostLifeThisLevel = false;
            onLevelStarted?.Invoke(sceneIndex);
            _hearts = heartAnimators.Length;
            foreach (var animator in heartAnimators)
            {
                animator.ResetControllerState();
            }
        }
    }

    private static float GetLevelDuration(int sceneIndex)
    {
        var levelDuration = 0f;
        var spawnEvents = Resources.LoadAll<SpawnEvent>($"Levels/{sceneIndex}");

        foreach (var spawnEvent in spawnEvents)
        {
            levelDuration = Mathf.Max(levelDuration, spawnEvent.time);
        }

        return levelDuration + 6f;
    }

}
