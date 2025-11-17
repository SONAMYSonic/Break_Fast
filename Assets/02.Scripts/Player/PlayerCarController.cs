using UnityEngine;

public enum PlayerState
{
    Idle,
    Moving,
    HitStun
}

public class PlayerCarController : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private Transform leftPos;
    [SerializeField] private Transform centerPos;
    [SerializeField] private Transform rightPos;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 15f;

    [Header("Player Stats")]
    [SerializeField] private int maxHp = 3;
    [SerializeField] private float comboResetTime = 3f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float hitCheckRadius = 1.0f;

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    public int CurrentHp { get; private set; }
    public int Combo { get; private set; }

    // 0 = Left, 1 = Center, 2 = Right
    private int _currentSlotIndex = 1;
    private Transform[] _slots;
    private Transform _targetPos;
    private float _comboTimer;
    private bool _hitDuringThisMove;

    private GameManager _gm;
    private CameraShake _shake;

    private void Awake()
    {
        _slots = new[] { leftPos, centerPos, rightPos };

        _currentSlotIndex = 1; // 시작을 Center로 가정
        if (centerPos != null)
            transform.position = centerPos.position;

        CurrentHp = maxHp;

        _gm = FindFirstObjectByType<GameManager>();
        _shake = FindFirstObjectByType<CameraShake>();
    }

    private void Update()
    {
        HandleInput();
        HandleMovement();
        HandleComboTimer();
    }

    private void HandleInput()
    {
        if (CurrentState != PlayerState.Idle) return;

        // 왼쪽 한 칸
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            TryMove(-1);
        }
        // 오른쪽 한 칸
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            TryMove(1);
        }
        // 가운데로 이동(강제로 Center로)
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            TryMoveToCenter();
        }
    }

    private void TryMove(int dir)
    {
        int newIndex = Mathf.Clamp(_currentSlotIndex + dir, 0, 2);
        if (newIndex == _currentSlotIndex) return;

        _targetPos = _slots[newIndex];
        _hitDuringThisMove = false;
        CurrentState = PlayerState.Moving;
    }

    private void TryMoveToCenter()
    {
        int centerIndex = 1;
        if (_currentSlotIndex == centerIndex) return;

        _targetPos = _slots[centerIndex];
        _hitDuringThisMove = false;
        CurrentState = PlayerState.Moving;
    }

    private void HandleMovement()
    {
        if (CurrentState != PlayerState.Moving) return;
        if (_targetPos == null) { CurrentState = PlayerState.Idle; return; }

        // 먼저 이동
        Vector3 nextPos = Vector3.MoveTowards(
            transform.position,
            _targetPos.position,
            moveSpeed * Time.deltaTime
        );
        transform.position = nextPos;

        // *** 이동 중 충돌 체크 → 중간에 적 있으면 그 자리에서 바로 박치기 후 멈춤 ***
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            hitCheckRadius,
            enemyLayer
        );

        if (hits.Length > 0)
        {
            foreach (var h in hits)
            {
                var enemy = h.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.OnHitByPlayer();
                }
            }

            _hitDuringThisMove = true;
            AddCombo();
            _gm?.OnEnemyHit(Combo);
            _shake?.Shake();

            // 여기서 바로 정지 (목적지까지 안 가고 충돌 지점에서 멈춤)
            CurrentState = PlayerState.Idle;
            _targetPos = null;

            // 슬롯 인덱스는: 근처 슬롯으로 스냅 (가장 가까운 슬롯 찾기)
            UpdateSlotIndexByPosition();
            return;
        }

        // 충돌 없이 목적지 도착한 경우
        if (Vector3.Distance(transform.position, _targetPos.position) <= 0.01f)
        {
            transform.position = _targetPos.position;
            CurrentState = PlayerState.Idle;
            _targetPos = null;

            // 슬롯 인덱스 업데이트
            UpdateSlotIndexByPosition();

            // 이번 이동 동안 한 번도 적을 못 맞췄다면 → 콤보 리셋
            if (!_hitDuringThisMove)
            {
                ResetCombo();
            }
        }
    }

    private void UpdateSlotIndexByPosition()
    {
        // 현재 위치에서 가장 가까운 슬롯을 찾아 인덱스 업데이트
        float bestDist = float.MaxValue;
        int bestIndex = _currentSlotIndex;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null) continue;
            float d = Vector3.Distance(transform.position, _slots[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                bestIndex = i;
            }
        }

        _currentSlotIndex = bestIndex;
    }

    private void HandleComboTimer()
    {
        if (Combo <= 0) return;

        _comboTimer -= Time.deltaTime;
        if (_comboTimer <= 0f)
        {
            ResetCombo();
        }
    }

    private void AddCombo()
    {
        Combo++;
        _comboTimer = comboResetTime;
        _gm?.OnComboChanged(Combo);
    }

    private void ResetCombo()
    {
        if (Combo <= 0) return;
        Combo = 0;
        _gm?.OnComboChanged(Combo);
    }

    public void TakeDamage(int dmg)
    {
        CurrentHp -= dmg;
        _gm?.OnHpChanged(CurrentHp, maxHp);

        if (CurrentHp <= 0)
        {
            _gm?.OnPlayerDead();
        }

        // TODO: 피격 연출 (깜빡임, 밀려남 등)
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitCheckRadius);
    }
}
