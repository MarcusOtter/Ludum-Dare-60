using UnityEngine;
using UnityEngine.SceneManagement;

public class UiActions : MonoBehaviour
{
    public void SetScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
