using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private const float IdleHitDamageFactor = 0.5f;   // 플레이어가 가만히 있을 때 박으면 대미지 절반

    [Header("Score")]
    [SerializeField] private int _scoreValue = 100;

    [Header("Enemy Stats")]
    [SerializeField] private int _maxHp = 10;

    [SerializeField] private int _currentHp;
    private bool _isDead;

    private GameManager _gameManager;

    public int CurrentHp => _currentHp;
    public bool IsDead => _isDead;

    private void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _currentHp = _maxHp;
    }

    private void Update()
    {
        if (_isDead)
        {
            return;
        }
    }

    public void OnHitByPlayer(int baseDamage, bool playerWasMoving, bool isUltimateAttack)
    {
        if (_isDead)
        {
            return;
        }

        int damage;

        if (isUltimateAttack)
        {
            // 궁극기면 무조건 한방
            damage = _maxHp;
        }
        else
        {
            damage = CalculateDamage(baseDamage, playerWasMoving);
        }

        ApplyDamage(damage);
    }


    private int CalculateDamage(int baseDamage, bool playerWasMoving)
    {
        if (playerWasMoving)
        {
            return baseDamage;
        }

        // 플레이어가 가만히 있을 때 들이받은 경우: 대미지 절반
        var scaledDamage = Mathf.RoundToInt(baseDamage * IdleHitDamageFactor);
        return Mathf.Max(scaledDamage, 1); // 최소 1은 보장
    }

    private void ApplyDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        _currentHp -= damage;

        if (_currentHp <= 0)
        {
            _currentHp = 0;
            Die();
        }
        else
        {
            // TODO: 피격 이펙트, 색 반짝임, 히트 애니메이션 등
        }
    }

    private void Die()
    {
        _isDead = true;

        _gameManager?.SpawnHitEffect(transform.position);
        _gameManager?.OnEnemyDestroyed(_scoreValue, transform.position);

        Destroy(gameObject);
    }
}
