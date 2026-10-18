using UnityEngine;

public class UiActions : MonoBehaviour
{
    public void SetScene(int index)
    {
        LevelManager.Instance.LoadLevel(index);
    }

    public void ReloadScene()
    {
        LevelManager.Instance.LoadLevel(SimpleSceneManager.CurrentSceneIndex);
    }

    public void LoadNextScene()
    {
        if (SimpleSceneManager.HasNextScene())
        {
            LevelManager.Instance.LoadLevel(SimpleSceneManager.CurrentSceneIndex + 1);
        }
    }

    public void SetMasterVolume(float volume)
    {
        AudioPlayer.Instance.SetMasterVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        MusicPlayer.Instance.SetVolume(volume);
    }
}
