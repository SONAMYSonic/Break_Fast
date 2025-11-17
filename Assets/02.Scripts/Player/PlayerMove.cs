using UnityEngine;

public enum PlayerState
{
    Idle,
    Moving,
    HitStun
}

public class PlayerMove : MonoBehaviour
{
    private const int LeftLaneIndex = 0;
    private const int CenterLaneIndex = 1;
    private const int RightLaneIndex = 2;

    private const float PositionTolerance = 0.01f;

    [Header("Lane Positions")]
    [SerializeField] private Transform _leftPosition;
    [SerializeField] private Transform _centerPosition;
    [SerializeField] private Transform _rightPosition;

    [Header("Move Settings")]
    [SerializeField] private float _moveSpeed = 15f;

    [Header("Collision / Hit Box")]
    [SerializeField] private LayerMask _enemyLayerMask;
    [SerializeField] private Vector2 _hitBoxSize = new Vector2(1.5f, 1.0f);
    [SerializeField] private float _idleHitCooldown = 0.3f;

    [Header("Knockback Settings")]
    [SerializeField] private float _knockbackDistance = 0.7f;
    [SerializeField] private float _knockbackDuration = 0.08f;

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    private readonly Transform[] _lanes = new Transform[3];
    private int _currentLaneIndex = CenterLaneIndex;
    private int _targetLaneIndex = CenterLaneIndex;

    private Transform _targetPosition;
    private float _idleHitTimer;
    private bool _hitDuringThisMove;

    private PlayerCarController _player;

    private Vector3 _knockbackStartPosition;
    private Vector3 _knockbackEndPosition;
    private float _knockbackTimer;

    private void Awake()
    {
        _lanes[LeftLaneIndex] = _leftPosition;
        _lanes[CenterLaneIndex] = _centerPosition;
        _lanes[RightLaneIndex] = _rightPosition;

        _currentLaneIndex = CenterLaneIndex;

        if (_centerPosition != null)
        {
            transform.position = _centerPosition.position;
        }

        _player = GetComponent<PlayerCarController>();
        if (_player == null)
        {
            Debug.LogError("[PlayerMove] PlayerCarController not found on the same GameObject.");
        }
    }

    private void Update()
    {
        HandleInput();

        switch (CurrentState)
        {
            case PlayerState.Moving:
                HandleMovement();
                break;

            case PlayerState.HitStun:
                HandleKnockback();
                break;
        }

        HandleIdleCollision();
    }


    // ---------------- Input ----------------

    private void HandleInput()
    {
        if (CurrentState != PlayerState.Idle)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveOneLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveOneLane(1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            MoveToCenterLane();
        }
    }

    private void MoveOneLane(int direction)
    {
        var desiredIndex = Mathf.Clamp(_currentLaneIndex + direction, LeftLaneIndex, RightLaneIndex);

        if (desiredIndex == _currentLaneIndex)
        {
            // 논리상 이 레인에 있지만, 실제 위치가 어긋나 있다면 다시 그 레인으로 정렬
            if (IsAtLanePosition(_currentLaneIndex) == false)
            {
                SetMoveTarget(_currentLaneIndex);
            }

            return;
        }

        SetMoveTarget(desiredIndex);
    }

    private void MoveToCenterLane()
    {
        if (_currentLaneIndex == CenterLaneIndex)
        {
            if (IsAtLanePosition(CenterLaneIndex) == false)
            {
                SetMoveTarget(CenterLaneIndex);
            }

            return;
        }

        SetMoveTarget(CenterLaneIndex);
    }

    private void SetMoveTarget(int laneIndex)
    {
        var laneTransform = _lanes[laneIndex];
        if (laneTransform == null)
        {
            return;
        }

        _targetLaneIndex = laneIndex;
        _targetPosition = laneTransform;
        _hitDuringThisMove = false;
        CurrentState = PlayerState.Moving;
    }

    // ---------------- Movement & Attack ----------------

    private void HandleMovement()
    {
        if (CurrentState != PlayerState.Moving || _targetPosition == null)
        {
            return;
        }

        var nextPosition = Vector3.MoveTowards(
            transform.position,
            _targetPosition.position,
            _moveSpeed * Time.deltaTime);

        transform.position = nextPosition;

        // 이동 중 박치기
        if (TryHitEnemyWhileMoving())
        {
            return;
        }

        // 적을 못 만나고 목표 레인 도착
        var distanceToTarget = Vector3.Distance(transform.position, _targetPosition.position);
        if (distanceToTarget <= PositionTolerance)
        {
            transform.position = _targetPosition.position;
            _currentLaneIndex = _targetLaneIndex;

            CurrentState = PlayerState.Idle;
            _targetPosition = null;
        }
    }

    private bool TryHitEnemyWhileMoving()
    {
        var hits = Physics2D.OverlapBoxAll(
            transform.position,
            _hitBoxSize,
            0f,
            _enemyLayerMask);

        if (hits.Length == 0)
        {
            return false;
        }

        // 넉백 방향 계산용 (가장 먼저 맞은 적 기준)
        float? contactDirection = null;

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyController>();
            if (enemy == null)
            {
                continue;
            }

            // 1) 데미지 계산
            enemy.OnHitByPlayer(_player.AttackPower, true);

            // 2) 적 넉백
            var enemyMove = hit.GetComponent<EnemyMove>();
            if (enemyMove != null)
            {
                if (contactDirection == null)
                {
                    // 적이 플레이어의 오른쪽에 있으면 → 적은 오른쪽(+1)으로 넉백
                    // 적이 왼쪽에 있으면 → 적은 왼쪽(-1)으로 넉백
                    contactDirection =
                        enemy.transform.position.x - transform.position.x > 0f
                            ? 1f
                            : -1f;
                }

                enemyMove.StartKnockback(contactDirection.Value);
            }
        }

        // 3) 플레이어 자기 연출
        _hitDuringThisMove = true;
        _player?.OnDashHitEnemy();

        // 4) 공격 직후에는 Idle 피격 판정 잠시 비활성화
        _idleHitTimer = _idleHitCooldown;

        // 5) 플레이어 넉백 (대시 반대 방향)
        if (contactDirection != null)
        {
            // 적이 오른쪽(+1)으로 밀려났다면 → 플레이어는 왼쪽(-1)으로 넉백
            // 적이 왼쪽(-1)으로 밀려났다면 → 플레이어는 오른쪽(+1)으로 넉백
            float playerKnockDirection = -contactDirection.Value;
            StartKnockback(playerKnockDirection);
        }
        else
        {
            // 혹시 모를 방어: 적이 하나도 없는데 여기 들어온 경우
            CurrentState = PlayerState.Idle;
        }

        _targetPosition = null;
        _currentLaneIndex = _targetLaneIndex;

        return true;
    }

    // ---------------- Idle Hit (가만히 있을 때 들이받힘) ----------------

    private void HandleIdleCollision()
    {
        if (CurrentState != PlayerState.Idle)
        {
            return;
        }

        if (_idleHitTimer > 0f)
        {
            _idleHitTimer -= Time.deltaTime;
            return;
        }

        var hits = Physics2D.OverlapBoxAll(
            transform.position,
            _hitBoxSize,
            0f,
            _enemyLayerMask);

        if (hits.Length == 0)
        {
            return;
        }

        EnemyController enemyForKnockback = null;
        EnemyMove enemyMoveForKnockback = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyController>();
            if (enemy == null)
            {
                continue;
            }

            // 플레이어는 가만히 있고, 적이 들이받은 상황 → 적은 반값 데미지
            enemy.OnHitByPlayer(_player.AttackPower, false);

            var distance = Mathf.Abs(hit.transform.position.x - transform.position.x);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                enemyForKnockback = enemy;
                enemyMoveForKnockback = hit.GetComponent<EnemyMove>();
            }
        }

        _player?.OnIdleHitByEnemy();

        if (enemyForKnockback != null)
        {
            // 플레이어 기준 넉백 방향
            float playerKnockDirection =
                transform.position.x - enemyForKnockback.transform.position.x > 0f
                    ? 1f
                    : -1f;

            // 플레이어 넉백
            StartKnockback(playerKnockDirection);

            // 적 넉백 (플레이어와 반대 방향)
            if (enemyMoveForKnockback != null)
            {
                float enemyKnockDirection = -playerKnockDirection;
                enemyMoveForKnockback.StartKnockback(enemyKnockDirection);
            }
        }

        _idleHitTimer = _idleHitCooldown;
    }


    // ---------------- Helpers ----------------

    private bool IsAtLanePosition(int laneIndex)
    {
        var laneTransform = _lanes[laneIndex];
        if (laneTransform == null)
        {
            return false;
        }

        var distance = Vector3.Distance(transform.position, laneTransform.position);
        return distance <= PositionTolerance;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, _hitBoxSize);
    }

    private void StartKnockback(float direction)
    {
        _knockbackStartPosition = transform.position;

        _knockbackEndPosition = _knockbackStartPosition +
                                new Vector3(_knockbackDistance * direction, 0f, 0f);

        _knockbackTimer = _knockbackDuration;
        CurrentState = PlayerState.HitStun;

        // 이동 중이었을 가능성은 없지만, 혹시 모를 이전 타겟 클리어
        _targetPosition = null;
    }

    private void HandleKnockback()
    {
        if (_knockbackTimer <= 0f || _knockbackDuration <= 0f)
        {
            CurrentState = PlayerState.Idle;
            return;
        }

        _knockbackTimer -= Time.deltaTime;

        var t = 1f - (_knockbackTimer / _knockbackDuration);
        t = Mathf.Clamp01(t);

        var nextPosition = Vector3.Lerp(_knockbackStartPosition, _knockbackEndPosition, t);
        transform.position = nextPosition;

        if (_knockbackTimer <= 0f)
        {
            CurrentState = PlayerState.Idle;
        }
    }

}
