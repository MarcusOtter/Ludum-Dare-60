using UnityEngine;

[CreateAssetMenu(menuName = "Custom audio clip")]
public class CustomAudioClip : ScriptableObject
{
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] [Range(0, 2)] private float maxPitch = 1f;
    [SerializeField] [Range(0, 1)] private float maxVolume = 0.5f;
    [SerializeField] [Range(0, 2)] private float minPitch = 1f;
    [SerializeField] [Range(0, 1)] private float minVolume = 0.5f;

    public AudioClip GetClip()
    {
        return audioClips.Length == 0
            ? null
            : audioClips[Random.Range(0, audioClips.Length)];
    }

    public float GetVolume()
    {
        return Random.Range(minVolume, maxVolume);
    }

    public float GetPitch()
    {
        return Random.Range(minPitch, maxPitch);
    }
}