using UnityEngine;
using UnityEngine.InputSystem;

public class FollowMouse : MonoBehaviour
{
    [SerializeField] private bool lockX;
    [SerializeField] private bool lockY;
    [SerializeField] private Vector2 rangeX = new(-2f, 2f);
    
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
        targetPosition.x = Mathf.Clamp(targetPosition.x, rangeX.x, rangeX.y);
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
