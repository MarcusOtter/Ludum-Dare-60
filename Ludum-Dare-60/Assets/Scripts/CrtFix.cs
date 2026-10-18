using UnityEngine;

public class CrtFix : MonoBehaviour
{
    private void Start()
    {
        foreach (var canvas in FindObjectsByType<Canvas>())
        {
            canvas.worldCamera = Camera.main;
        }
    }
}
