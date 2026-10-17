using UnityEngine;
using UnityEngine.InputSystem;

public class FollowMouse : MonoBehaviour
{
    [SerializeField] private bool lockX = false;
    [SerializeField] private bool lockY = false;
    
    private Camera _mainCamera;
    
    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        var mousePos = (Vector3)Mouse.current.position.ReadValue();
        mousePos.z = _mainCamera.transform.position.z * -1;

        var targetPosition = _mainCamera.ScreenToWorldPoint(mousePos);
        var currentPosition = transform.position;

        if (lockX)
        {
            targetPosition.x = currentPosition.x;
        }

        if (lockY)
        {
            targetPosition.y = currentPosition.y;
        }

        transform.position = targetPosition;
    }
}
