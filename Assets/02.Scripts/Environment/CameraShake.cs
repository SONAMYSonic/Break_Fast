using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private float magnitude = 0.2f;

    private Coroutine _shakeRoutine;
    private CameraZoomController _zoomController;

    private void Awake()
    {
        _zoomController = GetComponent<CameraZoomController>();
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
            Vector3 offset = new Vector3(offsetX, offsetY, 0f);

            if (_zoomController != null)
            {
                _zoomController.SetShakeOffset(offset);
            }
            else
            {
                // 혹시 줌컨트롤러가 없을 때를 대비한 fallback (옵션)
                transform.localPosition += offset;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (_zoomController != null)
        {
            _zoomController.SetShakeOffset(Vector3.zero);
        }

        _shakeRoutine = null;
    }
}
