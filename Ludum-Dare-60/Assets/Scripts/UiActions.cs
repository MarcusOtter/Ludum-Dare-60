using UnityEngine;

public class UiActions : MonoBehaviour
{
    public void SetScene(int index)
    {
        LevelManager.Instance.LoadLevel(index);
    }
}
