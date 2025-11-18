using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform _leftSpawnPoint;
    [SerializeField] private Transform _rightSpawnPoint;

    [Header("Enemy Target")]
    [SerializeField] private Transform _centerTargetPosition;   // 씬의 CenterPos 같은 지점

    [Header("Enemy Prefab")]
    [SerializeField] public GameObject[] EnemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float _startSpawnInterval = 2f;
    [SerializeField] private float _minSpawnInterval = 0.4f;
    [SerializeField] private float _difficultyRampDuration = 60f;   // 이 시간 동안 점점 스폰 간격 감소

    private float _spawnTimer;
    private float _elapsedTime;

    private bool _isActive = true;

    public bool IsActive => _isActive;

    private void Start()
    {
        _spawnTimer = _startSpawnInterval;
    }

    private void Update()
    {
        if (_isActive == false)
        {
            return;
        }

        _elapsedTime += Time.deltaTime;
        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f)
        {
            SpawnEnemy();
            _spawnTimer = CalculateCurrentInterval();
        }
    }

    private float CalculateCurrentInterval()
    {
        if (_difficultyRampDuration <= 0f)
        {
            return _minSpawnInterval;
        }

        var t = Mathf.Clamp01(_elapsedTime / _difficultyRampDuration);
        var interval = Mathf.Lerp(_startSpawnInterval, _minSpawnInterval, t);
        return interval;
    }

    private void SpawnEnemy()
    {
        var spawnOnLeft = Random.value < 0.5f;
        var spawnPoint = spawnOnLeft ? _leftSpawnPoint : _rightSpawnPoint;

        int enemyIndex = Random.Range(0, EnemyPrefabs.Length);

        var enemyObject = Instantiate(
            EnemyPrefabs[enemyIndex],
            spawnPoint.position,
            Quaternion.identity);

        var enemyController = enemyObject.GetComponent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogWarning("[EnemySpawner] Spawned enemy does not have EnemyController.");
            return;
        }
    }

    public void SetActive(bool isActive)
    {
        _isActive = isActive;
    }

    public void ResetSpawner()
    {
        _elapsedTime = 0f;
        _spawnTimer = _startSpawnInterval;
    }
}
