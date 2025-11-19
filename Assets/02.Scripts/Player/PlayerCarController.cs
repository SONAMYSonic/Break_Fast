using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int _maxHp = 100;
    [SerializeField] private int _attackPower = 10;

    [Header("Damage Values")]
    [SerializeField] private int _selfDamageOnAttack = 1;
    [SerializeField] private int _selfDamageOnIdleHit = 10;

    [Header("Combo Settings")]
    [SerializeField] private float _comboResetTime = 3f;

    [Header("Ultimate Settings")]
    [SerializeField] private int _ultimateRequiredCombo = 20;   // 궁극기 필요 콤보
    [SerializeField] private float _ultimateDuration = 5f;      // 궁극기 지속 시간
    public GameObject UltimateTrailEffect;                     // 궁극기 이펙트 오브젝트

    [SerializeField] private int _currentHp;
    public int Combo { get; private set; }
    public int AttackPower => _attackPower;

    public bool IsUltimateActive { get; private set; }   // 외부에서 읽기용

    private float _comboTimer;
    private float _ultimateTimer;
    private bool _ultimateReady;                         // 콤보로 사용 가능 여부

    private GameManager _gameManager;
    private CameraShake _cameraShake;
    private CameraZoomController _cameraZoom;

    private void Awake()
    {
        _currentHp = _maxHp;
        _gameManager = FindFirstObjectByType<GameManager>();
        _cameraShake = FindFirstObjectByType<CameraShake>();
        _gameManager?.OnHpChanged(_currentHp, _maxHp);
        _cameraZoom = FindFirstObjectByType<CameraZoomController>();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        UpdateComboTimer(dt);
        UpdateUltimate(dt);
    }

    // ---------------- 궁극기 로직 ----------------

    private void UpdateUltimate(float deltaTime)
    {
        // 이미 궁극기 모드일 때 → 타이머 감소
        if (IsUltimateActive)
        {
            _ultimateTimer -= deltaTime;
            if (_ultimateTimer <= 0f)
            {
                EndUltimate();
            }
            return;
        }

        // 궁극기 사용 가능 여부 계산 (콤보 기준)
        bool canUse = Combo >= _ultimateRequiredCombo;
        if (canUse != _ultimateReady)
        {
            _ultimateReady = canUse;
            _gameManager?.OnUltimateReadyChanged(_ultimateReady); // UI에 알림 (SPACE 아이콘 ON/OFF)
        }

        // 준비됐고, 스페이스 눌렀으면 → 바로 궁극기 발동
        if (_ultimateReady && Input.GetKeyDown(KeyCode.Space))
        {
            ActivateUltimateImmediately();
        }
    }

    /// <summary>
    /// 연출 대기 없이, UI만 띄우고 바로 궁극기 발동
    /// </summary>
    private void ActivateUltimateImmediately()
    {
        // SPACE UI 숨김
        _ultimateReady = false;
        _gameManager?.OnUltimateReadyChanged(false);

        // 콤보 소진
        ResetCombo();

        // 오른쪽 아래 필살기 UI 표시 + 보이스 재생
        _gameManager?.ShowUltimateCutIn(true);
        SoundManager.Instance?.PlayUltimateVoice();

        // 궁극기 효과 시작
        BeginUltimate();
    }

    private void BeginUltimate()
    {
        IsUltimateActive = true;
        _ultimateTimer = _ultimateDuration;
        // 필요하면 여기서 VFX 시작 등 추가 가능
        _cameraZoom?.SetUltimateZoom(true);
        UltimateTrailEffect.SetActive(true);
    }

    private void EndUltimate()
    {
        IsUltimateActive = false;
        // 필살기 UI 끄기
        _gameManager?.ShowUltimateCutIn(false);
        // 종료 VFX/사운드 필요하면 여기서
        _cameraZoom?.SetUltimateZoom(false);
        UltimateTrailEffect.SetActive(false);
    }

    // ---------------- 콤보/데미지 ----------------

    private void UpdateComboTimer(float deltaTime)
    {
        if (Combo <= 0)
        {
            return;
        }

        _comboTimer -= deltaTime;
        if (_comboTimer <= 0f)
        {
            ResetCombo();
        }
    }

    // 이동 중 박치기 성공했을 때 호출
    public void OnDashHitEnemy()
    {
        AddCombo();
        TakeDamage(_selfDamageOnAttack);
        _gameManager?.OnEnemyHit(Combo);
        _cameraShake?.Shake();
        SoundManager.Instance?.PlaySfx(SfxType.DashHit);
    }

    // 가만히 있다가 적에게 들이받힌 경우
    public void OnIdleHitByEnemy()
    {
        TakeDamage(_selfDamageOnIdleHit);
        _cameraShake?.Shake();
        ResetCombo();
        Debug.Log("으악! 적에게 들이받혔습니다!");
        SoundManager.Instance?.PlaySfx(SfxType.IdleHit);
        SoundManager.Instance?.PlayerHitVoice();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        _currentHp -= damage;
        if (_currentHp < 0)
        {
            _currentHp = 0;
        }

        _gameManager?.OnHpChanged(_currentHp, _maxHp);

        if (_currentHp <= 0)
        {
            _gameManager?.OnPlayerDead();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || _currentHp <= 0)
        {
            // 죽어있거나 0이하 힐 값이면 무시
            return;
        }

        _currentHp = Mathf.Clamp(_currentHp + amount, 0, _maxHp);
        _gameManager?.OnHpChanged(_currentHp, _maxHp);
        SoundManager.Instance?.PlaySfx(SfxType.GetItem);
    }

    private void AddCombo()
    {
        Combo++;
        _comboTimer = _comboResetTime;
        _gameManager?.OnComboChanged(Combo);
    }

    public void ResetCombo()
    {
        if (Combo <= 0)
        {
            return;
        }

        Combo = 0;
        _gameManager?.OnComboChanged(Combo);
    }
}
