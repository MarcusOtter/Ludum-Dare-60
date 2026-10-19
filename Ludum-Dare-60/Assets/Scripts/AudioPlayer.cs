using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : Singleton<AudioPlayer>
{
    private const float InitialMasterVolume = 0.5f;

    [SerializeField] private CustomAudioClip bounce;
    [SerializeField] private CustomAudioClip compacting;
    [SerializeField] private CustomAudioClip damage;
    [SerializeField] private CustomAudioClip spawnedObject;
    [SerializeField] private CustomAudioClip success;

    private AudioSource _audioSource;
    private bool _isMuted;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            SetMasterVolume(InitialMasterVolume);
        }
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySoundEffect(CustomAudioClip clip)
    {
        PlayCustomAudioClip(clip);
    }

    public void PlayBounce() => PlaySoundEffect(bounce);
    public void PlayCompacting() => PlaySoundEffect(compacting);
    public void PlayDamage() => PlaySoundEffect(damage);
    public void PlaySpawnedObject() => PlaySoundEffect(spawnedObject);
    public void PlaySuccess() => PlaySoundEffect(success);

    public void StopAllSoundEffects()
    {
        // AudioPlayer persists across scenes, as do the temporary sources used
        // for pitch-varied SFX. Stop both the root one-shot source and all of
        // those children before a restart.
        foreach (var source in GetComponentsInChildren<AudioSource>(true))
        {
            source.Stop();
        }
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }

    public void Mute(bool mute)
    {
        if (mute == _isMuted)
        {
            return;
        }

        _isMuted = mute;
        _audioSource.mute = mute;

        foreach (var audioSources in FindObjectsByType<AudioSource>())
        {
            audioSources.mute = mute;
        }
    }

    // Returns clip length
    private float PlayCustomAudioClip(CustomAudioClip clip)
    {
        if (_isMuted || clip == null)
        {
            return 0f;
        }

        if (Mathf.Approximately(clip.GetPitch(), 1))
        {
            _audioSource.PlayOneShot(clip.GetClip(), clip.GetVolume());
            return 0f;
        }

        var source = SpawnNewAudioSource();
        return PlayClipOnAudioSource(source, clip);
    }


    // Returns clips length
    private static float PlayClipOnAudioSource(AudioSource source, CustomAudioClip clip)
    {
        var audioClip = clip.GetClip();
        source.volume = clip.GetVolume();
        source.pitch = clip.GetPitch();
        source.clip = audioClip;
        source.Play();

        Destroy(source.gameObject, clip.GetClip().length + 2f);
        return audioClip.length;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private static AudioSource SpawnNewAudioSource(string name = "Audio source")
    {
        var obj = new GameObject(name, typeof(AudioSource));
        obj.transform.parent = Instance.transform;
        return obj.GetComponent<AudioSource>();
    }
}
