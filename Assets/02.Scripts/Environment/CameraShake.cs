using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private float magnitude = 0.2f;

    private Vector3 _originalPos;
    private Coroutine _shakeRoutine;

    private void Awake()
    {
        _originalPos = transform.localPosition;
    }

    public void Shake()
    {
        if (_shakeRoutine != null)
        {
            StopCoroutine(_shakeRoutine);
        }
        _shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = _originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = _originalPos;
    }
}
