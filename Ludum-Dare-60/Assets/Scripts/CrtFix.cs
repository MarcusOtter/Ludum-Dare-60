using System.Collections;
using UnityEngine;

public class CrtFix : MonoBehaviour
{
    [SerializeField, Min(0f)] private float damageShakeStrength = 0.15f;

    private const float DamageShakeDuration = 0.25f;
    private Coroutine _damageShakeRoutine;

    private void Start()
    {
        foreach (var canvas in FindObjectsByType<Canvas>())
        {
            canvas.worldCamera = Camera.main;
        }
    }

    public void ShakeOnDamage()
    {
        if (_damageShakeRoutine != null)
        {
            StopCoroutine(_damageShakeRoutine);
        }

        _damageShakeRoutine = StartCoroutine(ShakeCamera());
    }

    private IEnumerator ShakeCamera()
    {
        var originalPosition = transform.localPosition;
        var elapsed = 0f;

        while (elapsed < DamageShakeDuration)
        {
            var offset = Random.insideUnitCircle * damageShakeStrength;
            transform.localPosition = originalPosition + new Vector3(offset.x, offset.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        _damageShakeRoutine = null;
    }
}
