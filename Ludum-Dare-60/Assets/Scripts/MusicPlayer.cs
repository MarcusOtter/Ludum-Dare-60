using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : Singleton<MusicPlayer>
{
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip levelStartedMusic;
    [SerializeField] private AudioClip gameRunningMusic;

    private AudioSource _audioSource;
    private Coroutine _musicRoutine;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
        {
            return;
        }

        _audioSource = GetComponent<AudioSource>();
        _audioSource.Stop();
        if (SimpleSceneManager.CurrentSceneIndex == 0)
        {
            Play(menuMusic, true);
        }
        else
        {
            _musicRoutine = StartCoroutine(PlayLevelMusic());
        }
    }

    private void OnEnable()
    {
        LevelManager.Instance.onLevelStarted.AddListener(HandleLevelStarted);
    }

    private void OnDisable()
    {
        LevelManager.Instance.onLevelStarted.RemoveListener(HandleLevelStarted);
    }

    private void HandleLevelStarted(int sceneIndex)
    {
        if (_musicRoutine != null)
        {
            StopCoroutine(_musicRoutine);
        }

        _musicRoutine = StartCoroutine(sceneIndex == 0 ? PlayMenuMusic() : PlayLevelMusic());
    }

    private IEnumerator PlayLevelMusic()
    {
        if (_audioSource.isPlaying)
        {
            yield return new WaitForSecondsRealtime(_audioSource.clip.length - _audioSource.time);
        }

        Play(levelStartedMusic, false);
        yield return new WaitForSecondsRealtime(levelStartedMusic.length);
        Play(gameRunningMusic, true);
        _musicRoutine = null;
    }

    private IEnumerator PlayMenuMusic()
    {
        if (_audioSource.isPlaying)
        {
            yield return new WaitForSecondsRealtime(_audioSource.clip.length - _audioSource.time);
        }

        Play(menuMusic, true);
        _musicRoutine = null;
    }

    private void Play(AudioClip clip, bool loop)
    {
        _audioSource.clip = clip;
        _audioSource.loop = loop;
        _audioSource.Play();
    }
}
