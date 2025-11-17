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
