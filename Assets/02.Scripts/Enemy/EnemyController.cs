using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private int damageToPlayer = 1;
    [SerializeField] private Transform targetPos; // 중앙 근처 포인트

    private bool _isDead = false;
    private GameManager _gameManager;
    private PlayerCarController _player;

    private void Awake()
    {
        _gameManager = Object.FindFirstObjectByType<GameManager>();
        _player = Object.FindFirstObjectByType<PlayerCarController>();
    }

    private void Update()
    {
        if (_isDead) return;
        MoveToCenter();
    }

    private void MoveToCenter()
    {
        if (targetPos == null)
            targetPos = _player.transform;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos.position,
            moveSpeed * Time.deltaTime
        );

        // ★ 더 이상 center 도착했다고 사라지지 않음
        // float dist = Vector3.Distance(transform.position, targetPos.position);
        // if (dist <= 0.05f)
        // {
        //     _player?.TakeDamage(damageToPlayer);
        //     Die(false);
        // }
    }

    public void OnHitByPlayer()
    {
        if (_isDead) return;
        _isDead = true;

        // 폭발 이펙트, 사운드
        _gameManager?.SpawnHitEffect(transform.position);

        // 점수는 GameManager.OnEnemyHit 쪽에서 처리 or 여기서 처리도 가능
        Destroy(gameObject);
    }

    private void Die(bool giveScore = false)
    {
        _isDead = true;
        if (giveScore)
        {
            _gameManager?.OnEnemyDestroyed(scoreValue);
        }
        Destroy(gameObject);
    }
}
