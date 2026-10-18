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

        if (_hearts <= 0)
        {
            onLevelEnded.Invoke(SimpleSceneManager.CurrentSceneIndex);
        }
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

        onLevelCompleted.Invoke(SimpleSceneManager.CurrentSceneIndex);
    }
    
    private void ChangeScene(int sceneIndex)
    {
        if (levelProgressImage != null)
        {
            levelProgressImage.fillAmount = 0f;
        }

        
        levelProgressImage.transform.parent.gameObject.SetActive(sceneIndex != 0);
        levelProgressText.SetActive(sceneIndex != 0);
        heartContainer.SetActive(sceneIndex != 0);
        

        if (sceneIndex == 0)
        {
            _levelDuration = 0f;
            onMainMenuLoaded.Invoke();
        }
        else
        {
            _levelStartTime = Time.time;
            _levelDuration = GetLevelDuration(sceneIndex);
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
