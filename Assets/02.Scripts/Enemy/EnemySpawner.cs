using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform leftSpawn;
    [SerializeField] private Transform rightSpawn;
    [SerializeField] private Transform centerTarget;
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float startInterval = 2f;
    [SerializeField] private float minInterval = 0.4f;
    [SerializeField] private float difficultyDuration = 60f; // 60ÃÊ µ¿¾È Á¡Á¡ »¡¶óÁü

    private float _timer;
    private float _elapsed;

    private bool _isActive = true;

    private void Update()
    {
        if (!_isActive) return;

        _elapsed += Time.deltaTime;
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            SpawnEnemy();
            _timer = GetCurrentInterval();
        }
    }

    private float GetCurrentInterval()
    {
        float t = Mathf.Clamp01(_elapsed / difficultyDuration);
        return Mathf.Lerp(startInterval, minInterval, t);
    }

    private void SpawnEnemy()
    {
        bool spawnLeft = Random.value < 0.5f;
        Transform spawnPoint = spawnLeft ? leftSpawn : rightSpawn;

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        EnemyController enemy = enemyObj.GetComponent<EnemyController>();
    }

    public void SetActive(bool isActive)
    {
        _isActive = isActive;
    }

    public void ResetSpawner()
    {
        _elapsed = 0f;
        _timer = startInterval;
    }
}
