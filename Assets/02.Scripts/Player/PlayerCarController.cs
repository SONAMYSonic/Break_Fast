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

    [SerializeField] private int _currentHp;
    public int Combo { get; private set; }
    public int AttackPower => _attackPower;

    private float _comboTimer;

    private GameManager _gameManager;
    private CameraShake _cameraShake;

    private void Awake()
    {
        _currentHp = _maxHp;
        _gameManager = FindFirstObjectByType<GameManager>();
        _cameraShake = FindFirstObjectByType<CameraShake>();
    }

    private void Update()
    {
        UpdateComboTimer(Time.deltaTime);
    }

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
