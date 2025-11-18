using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Knockback Settings")]
    [SerializeField] private float _knockbackDistance = 0.7f;
    [SerializeField] private float _knockbackDuration = 0.08f;

    private Transform _playerTransform;

    private Vector3 _knockbackStartPosition;
    private Vector3 _knockbackEndPosition;
    private float _knockbackTimer;
    private bool _isKnockback;

    private void Awake()
    {
        // 씬에서 PlayerCarController 자동 찾기
        var player = FindFirstObjectByType<PlayerCarController>();
        if (player != null)
        {
            _playerTransform = player.transform;

            // ★ 스폰 위치 기준으로 처음 바라볼 방향 세팅
            UpdateFacingToPlayerSide();
        }
        else
        {
            Debug.LogWarning("[EnemyMove] Player not found in scene.");
        }
    }

    private void Update()
    {
        if (_playerTransform == null)
        {
            return;
        }

        if (_isKnockback)
        {
            HandleKnockback();
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer()
    {
        var current = transform.position;
        var target = _playerTransform.position;

        var next = Vector3.MoveTowards(
            current,
            target,
            _moveSpeed * Time.deltaTime);

        transform.position = next;
    }

    /// <summary>
    /// 스폰된 위치(왼/오)에 따라 적 모델이 플레이어 쪽을 바라보도록 회전
    /// 프리팹 기본 방향이 "오른쪽(+X)"을 본다고 가정.
    /// </summary>
    private void UpdateFacingToPlayerSide()
    {
        if (_playerTransform == null) return;

        float playerX = _playerTransform.position.x;
        float myX = transform.position.x;

        // 내가 플레이어보다 오른쪽에 있으면 → 왼쪽(플레이어) 보도록 180도 회전
        if (myX > playerX)
        {
            // Y축 기준으로 180도 회전 (오른쪽 → 왼쪽)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            // 내가 왼쪽에 있으면 프리팹 기본 방향(오른쪽)을 유지
            // 필요하면 초기 로테이션값을 따로 저장해 두고 사용해도 됨
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    public void StartKnockback(float direction)
    {
        _knockbackStartPosition = transform.position;

        _knockbackEndPosition = _knockbackStartPosition +
                                new Vector3(_knockbackDistance * direction, 0f, 0f);

        _knockbackTimer = _knockbackDuration;
        _isKnockback = true;
    }

    private void HandleKnockback()
    {
        if (_knockbackTimer <= 0f || _knockbackDuration <= 0f)
        {
            _isKnockback = false;
            return;
        }

        _knockbackTimer -= Time.deltaTime;

        var t = 1f - (_knockbackTimer / _knockbackDuration);
        t = Mathf.Clamp01(t);

        var nextPosition = Vector3.Lerp(_knockbackStartPosition, _knockbackEndPosition, t);
        transform.position = nextPosition;

        if (_knockbackTimer <= 0f)
        {
            _isKnockback = false;
        }
    }
}
