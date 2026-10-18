using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LevelIntro : MonoBehaviour
{
    private static readonly int Fade = Animator.StringToHash("Fade");

    [SerializeField] private Sprite[] levelIntroSprites;
    [SerializeField] private SpriteRenderer levelIntroRenderer;
    
    private Animator  _animator;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        LevelManager.Instance.onLevelStarted.AddListener(PlayFade);
    }

    private void OnDisable()
    {
        LevelManager.Instance.onLevelStarted.RemoveListener(PlayFade);
    }
    
    private void PlayFade(int sceneIndex)
    {
        var introSprite = levelIntroSprites[sceneIndex - 1];
        levelIntroRenderer.sprite = introSprite;
        _animator.SetTrigger(Fade);
    }
}
