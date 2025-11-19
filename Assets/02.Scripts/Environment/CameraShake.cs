using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private float magnitude = 0.2f;

    [Header("플레이어 이동 시 흔들림")]
    [SerializeField] private float _playerMoveDuration = 0.05f;
    [SerializeField] private float _playerMoveMagnitude = 0.1f;

    private Coroutine _shakeRoutine;
    private CameraZoomController _zoomController;

    private void Awake()
    {
        _zoomController = GetComponent<CameraZoomController>();
    }

    /// <summary>
    /// 강한 일반 흔들림 (피격/박치기 등)
    /// </summary>
    public void Shake()
    {
        StartShake(duration, magnitude);
    }

    /// <summary>
    /// 플레이어 이동용 약한 흔들림
    /// </summary>
    public void ShakeOnPlayerMove()
    {
        StartShake(_playerMoveDuration, _playerMoveMagnitude);
    }

    // 공통 시작부
    private void StartShake(float shakeDuration, float shakeMagnitude)
    {
        if (_shakeRoutine != null)
        {
            StopCoroutine(_shakeRoutine);
        }
        _shakeRoutine = StartCoroutine(ShakeRoutine(shakeDuration, shakeMagnitude));
    }

    private IEnumerator ShakeRoutine(float shakeDuration, float shakeMagnitude)
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
            Vector3 offset = new Vector3(offsetX, offsetY, 0f);

            if (_zoomController != null)
            {
                _zoomController.SetShakeOffset(offset);
            }
            else
            {
                // 줌컨트롤러가 없으면 그냥 위치에 직접 적용 (임시용)
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
