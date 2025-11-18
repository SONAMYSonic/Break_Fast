using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int Score { get; private set; }
    public int BestScore { get; private set; }

    [SerializeField] private UIManager uiManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Item Drop Settings")]
    [SerializeField] private GameObject _healthItemPrefab;
    [SerializeField, Range(0f, 1f)] private float _healthItemDropChance = 0.1f;

    private bool _isGameOver = false;

    private void Start()
    {
        BestScore = PlayerPrefs.GetInt("BestScore", 0);
        uiManager.UpdateScore(Score, BestScore);
    }

    public void OnEnemyHit(int combo)
    {
        int baseScore = 100;
        int addScore = baseScore + (combo * 10);
        Score += addScore;
        uiManager.UpdateScore(Score, BestScore);
        uiManager.PopupScore(addScore);
    }

    public void OnEnemyDestroyed(int scoreValue, Vector3 enemyPosition)
    {
        Score += scoreValue;
        uiManager.UpdateScore(Score, BestScore);

        TrySpawnHealthItem(enemyPosition);
    }

    private void TrySpawnHealthItem(Vector3 pos)
    {
        if (_healthItemPrefab == null)
            return;

        // 10% 확률
        if (Random.value > _healthItemDropChance)
            return;

        Instantiate(_healthItemPrefab, pos, Quaternion.identity);
    }

    public void OnHpChanged(int current, int max)
    {
        uiManager.UpdateHp(current, max);
    }

    public void OnComboChanged(int combo)
    {
        uiManager.UpdateCombo(combo);
    }

    public void OnPlayerDead()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        enemySpawner.SetActive(false);

        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetInt("BestScore", BestScore);
        }

        uiManager.ShowGameOver(Score, BestScore);
    }

    public void SpawnHitEffect(Vector3 pos)
    {
        if (hitEffectPrefab == null) return;
        Instantiate(hitEffectPrefab, pos, Quaternion.identity);
    }

    private void Update()
    {
        if (_isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
