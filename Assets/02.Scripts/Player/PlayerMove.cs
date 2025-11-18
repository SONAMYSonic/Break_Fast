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
    [SerializeField] private float _moveSpeed = 20f;
    [SerializeField] private float _moveMaxDistanceX = 2f;  // 키 1번당 최대 대시 거리

    [Header("Hit Box Settings")]
    [SerializeField] private LayerMask _enemyLayerMask;
    [SerializeField] private Vector2 _hitBoxSize = new Vector2(1.5f, 1.0f);

    [Header("Idle Hit Settings")]
    [SerializeField] private float _idleHitCooldown = 0.25f;

    [Header("Knockback Settings")]
    [SerializeField] private float _knockbackDistance = 0.7f;
    [SerializeField] private float _knockbackDuration = 0.08f;

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    private readonly Transform[] _lanes = new Transform[3];
    private int _currentLaneIndex = CenterLaneIndex;
    private int _targetLaneIndex = CenterLaneIndex;

    private Transform _targetLanePosition;

    private Vector3 _dashStartPos;    // 대시 시작 지점
    private float _idleHitTimer;
    private bool _hitDuringDash;

    private PlayerCarController _player;

    // knockback
    private Vector3 _knockStart;
    private Vector3 _knockEnd;
    private float _knockTimer;

    private void Awake()
    {
        _lanes[LeftLaneIndex] = _leftPosition;
        _lanes[CenterLaneIndex] = _centerPosition;
        _lanes[RightLaneIndex] = _rightPosition;

        transform.position = _centerPosition.position;
        _player = GetComponent<PlayerCarController>();
    }

    private void Update()
    {
        HandleInput();

        switch (CurrentState)
        {
            case PlayerState.Moving:
                HandleDashMovement();
                break;
            case PlayerState.HitStun:
                HandleKnockback();
                break;
        }

        HandleIdleCollision();
    }

    // ---------------- INPUT ----------------

    private void HandleInput()
    {
        if (CurrentState != PlayerState.Idle)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            DashDirection(-1);

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            DashDirection(1);

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            DashToCenter();
    }

    // 키 방향에 따라 목표 레인을 결정하고 대시 시작
    private void DashDirection(int dir)
    {
        int desiredLane = _currentLaneIndex;

        if (_currentLaneIndex == LeftLaneIndex && dir > 0)
            desiredLane = RightLaneIndex;
        else if (_currentLaneIndex == RightLaneIndex && dir < 0)
            desiredLane = LeftLaneIndex;
        else
            desiredLane = Mathf.Clamp(_currentLaneIndex + dir, 0, 2);

        StartDash(desiredLane);
    }

    private void DashToCenter()
    {
        StartDash(CenterLaneIndex);
    }

    // ---------------- DASH START ----------------

    private void StartDash(int laneIndex)
    {
        _targetLaneIndex = laneIndex;
        _targetLanePosition = _lanes[laneIndex];

        _dashStartPos = transform.position;
        _hitDuringDash = false;

        CurrentState = PlayerState.Moving;
    }

    // ---------------- DASH MOVEMENT ----------------

    private void HandleDashMovement()
    {
        if (_targetLanePosition == null)
            return;

        // 대시 이동
        Vector3 nextPos = Vector3.MoveTowards(
            transform.position,
            _targetLanePosition.position,
            _moveSpeed * Time.deltaTime);

        transform.position = nextPos;

        // 1) 이동 중 박치기 시도
        if (TryDashHit())
            return;

        // 2) 대시 이동 거리 초과 → MISS 판정
        float dashDistance = Mathf.Abs(transform.position.x - _dashStartPos.x);
        if (dashDistance >= _moveMaxDistanceX)
        {
            EndDash(miss: true);
            return;
        }

        // 3) 목표 레인 중심까지 도달 → MISS 여부 판정
        float distToLane = Vector3.Distance(transform.position, _targetLanePosition.position);
        if (distToLane <= PositionTolerance)
        {
            EndDash(!_hitDuringDash); // 히트 안 했으면 miss
        }
    }

    // ---------------- DASH HIT ----------------

    private bool TryDashHit()
    {
        var hits = Physics2D.OverlapBoxAll(
            transform.position,
            _hitBoxSize,
            0f,
            _enemyLayerMask);

        if (hits.Length == 0)
            return false;

        EnemyController closestEnemy = null;
        EnemyMove closestMove = null;
        float best = float.MaxValue;

        foreach (var h in hits)
        {
            var e = h.GetComponent<EnemyController>();
            if (e == null) continue;

            float d = Mathf.Abs(e.transform.position.x - transform.position.x);
            if (d < best)
            {
                best = d;
                closestEnemy = e;
                closestMove = h.GetComponent<EnemyMove>();
            }
        }

        if (closestEnemy == null)
            return false;

        // HIT 처리
        closestEnemy.OnHitByPlayer(_player.AttackPower, true);
        _player.OnDashHitEnemy();
        _hitDuringDash = true;

        // 적/플레이어 넉백
        float dir = Mathf.Sign(closestEnemy.transform.position.x - transform.position.x);

        if (closestMove != null)
            closestMove.StartKnockback(dir);

        StartKnockback(-dir);

        // ★ 여기서 더 이상 EndDash(false)를 호출하지 않는다!
        //    → HitStun 상태 동안 넉백 모션이 보이도록 유지.

        // 대시는 여기서 끝난 것으로 처리: 목표 레인 타겟 제거 & 레인 인덱스만 업데이트
        _targetLanePosition = null;
        _currentLaneIndex = _targetLaneIndex;

        return true;
    }

    // ---------------- DASH END (MISS 전용) ----------------

    private void EndDash(bool miss)
    {
        CurrentState = PlayerState.Idle;
        _targetLanePosition = null;
        _currentLaneIndex = _targetLaneIndex;

        if (miss)
            _player.ResetCombo();
    }

    // ---------------- IDLE HIT ----------------

    private void HandleIdleCollision()
    {
        if (CurrentState != PlayerState.Idle)
            return;

        if (_idleHitTimer > 0)
        {
            _idleHitTimer -= Time.deltaTime;
            return;
        }

        var hits = Physics2D.OverlapBoxAll(transform.position, _hitBoxSize, 0, _enemyLayerMask);
        if (hits.Length == 0)
            return;

        EnemyController hitEnemy = null;
        EnemyMove hitMove = null;
        float best = float.MaxValue;

        foreach (var h in hits)
        {
            var e = h.GetComponent<EnemyController>();
            if (e == null) continue;

            // 플레이어가 가만히 있을 때 들이받힌 상황 → 반값 데미지
            e.OnHitByPlayer(_player.AttackPower, false);

            float d = Mathf.Abs(e.transform.position.x - transform.position.x);
            if (d < best)
            {
                best = d;
                hitEnemy = e;
                hitMove = h.GetComponent<EnemyMove>();
            }
        }

        _player.OnIdleHitByEnemy();

        if (hitEnemy != null)
        {
            float dir = Mathf.Sign(transform.position.x - hitEnemy.transform.position.x);
            StartKnockback(dir);

            if (hitMove != null)
                hitMove.StartKnockback(-dir);
        }

        _idleHitTimer = _idleHitCooldown;
    }

    // ---------------- KNOCKBACK ----------------

    private void StartKnockback(float dir)
    {
        _knockStart = transform.position;
        _knockEnd = _knockStart + new Vector3(_knockbackDistance * dir, 0, 0);

        _knockTimer = _knockbackDuration;
        CurrentState = PlayerState.HitStun;
    }

    private void HandleKnockback()
    {
        if (_knockTimer <= 0)
        {
            CurrentState = PlayerState.Idle;
            return;
        }

        _knockTimer -= Time.deltaTime;

        float t = 1f - (_knockTimer / _knockbackDuration);
        transform.position = Vector3.Lerp(_knockStart, _knockEnd, t);

        if (_knockTimer <= 0)
            CurrentState = PlayerState.Idle;
    }

    // ---------------- DEBUG ----------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, _hitBoxSize);
    }
}
