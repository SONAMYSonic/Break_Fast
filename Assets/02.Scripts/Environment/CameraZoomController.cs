using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraZoomController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float _defaultSize = 2.5f;
    [SerializeField] private float _ultimateSize = 1.5f;
    [SerializeField] private float _zoomSpeed = 5f;

    [Header("Position Settings")]
    [SerializeField] private Vector3 _defaultLocalPos;   // 기본 위치
    [SerializeField] private Vector3 _ultimateLocalPos;  // 필살기 위치
    [SerializeField] private float _moveSpeed = 5f;

    private Camera _camera;

    private float _targetSize;
    private Vector3 _targetLocalPos;
    private Vector3 _currentBaseLocalPos;   // 줌/이동만 반영한 기준 위치
    private Vector3 _shakeOffset;           // CameraShake가 넣어줄 오프셋

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        if (_camera.orthographic && _defaultSize <= 0f)
            _defaultSize = _camera.orthographicSize;

        // 현재 위치를 기본 위치로
        _defaultLocalPos = transform.localPosition;
        _currentBaseLocalPos = _defaultLocalPos;

        _targetSize = _defaultSize;
        _targetLocalPos = _defaultLocalPos;
    }

    private void Update()
    {
        if (_camera == null || !_camera.orthographic)
            return;

        // 1) 줌
        _camera.orthographicSize = Mathf.Lerp(
            _camera.orthographicSize,
            _targetSize,
            Time.deltaTime * _zoomSpeed
        );

        // 2) 기본 위치 Lerp
        _currentBaseLocalPos = Vector3.Lerp(
            _currentBaseLocalPos,
            _targetLocalPos,
            Time.deltaTime * _moveSpeed
        );

        // 3) 기본 위치 + 쉐이크 오프셋 = 최종 위치
        transform.localPosition = _currentBaseLocalPos + _shakeOffset;
    }

    /// <summary>
    /// true = 필살기 줌인, false = 기본 줌
    /// </summary>
    public void SetUltimateZoom(bool isUltimate)
    {
        _targetSize = isUltimate ? _ultimateSize : _defaultSize;
        _targetLocalPos = isUltimate ? _ultimateLocalPos : _defaultLocalPos;
    }

    /// <summary>
    /// 카메라 흔들림용 추가 오프셋
    /// </summary>
    public void SetShakeOffset(Vector3 offset)
    {
        _shakeOffset = offset;
    }
}
