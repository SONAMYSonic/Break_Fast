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

    [Header("Game Start Settings")]
    [SerializeField] private float _fadeInDuration = 1.0f;     // 검은 화면 → 게임 화면
    [SerializeField] private float _countdownInterval = 1.0f;  // 3,2,1,GO 간 간격

    [SerializeField] private PlayerMove playerMove;

    private bool _isGameOver = false;

    private void Start()
    {
        BestScore = PlayerPrefs.GetInt("BestScore", 0);
        uiManager.UpdateScore(Score, BestScore);

        if (playerMove == null)
            playerMove = FindFirstObjectByType<PlayerMove>();

        enemySpawner.SetActive(false);
        if (playerMove != null)
            playerMove.SetCanControl(false);

        // 혹시 모를 자동 BGM을 안전하게 끄고 시작 (겹침 방지용)
        SoundManager.Instance?.StopBgm();

        StartCoroutine(GameStartRoutine());
    }


    private System.Collections.IEnumerator GameStartRoutine()
    {
        // 1) 처음엔 검은 화면 활성화
        uiManager.SetFadeActive(true);
        uiManager.SetFadeAlpha(1f);
        uiManager.SetCountdownText("");

        // 2) 페이드 인 (1 -> 0)
        float t = 0f;
        while (t < _fadeInDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / _fadeInDuration);
            uiManager.SetFadeAlpha(alpha);
            yield return null;
        }
        uiManager.SetFadeAlpha(0f);
        uiManager.SetFadeActive(false);

        // 3) 3, 2, 1, GO! 카운트다운 + 보이스

        uiManager.SetCountdownText("3");
        SoundManager.Instance?.PlayRandomCountdownVoice();
        yield return new WaitForSeconds(_countdownInterval);

        uiManager.SetCountdownText("2");
        yield return new WaitForSeconds(_countdownInterval);

        uiManager.SetCountdownText("1");
        yield return new WaitForSeconds(_countdownInterval);

        uiManager.SetCountdownText("GO!");
        SoundManager.Instance?.PlayRandomGoVoice();

        // ★ 여기서 BGM 시작!
        SoundManager.Instance?.PlayBgm();

        yield return new WaitForSeconds(_countdownInterval * 0.7f);

        uiManager.SetCountdownText("");

        // 4) 진짜 게임 시작
        enemySpawner.ResetSpawner();
        enemySpawner.SetActive(true);

        if (playerMove != null)
            playerMove.SetCanControl(true);
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
        playerMove.SetCanControl(false);

        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetInt("BestScore", BestScore);
        }

        uiManager.ShowGameOver(Score, BestScore);
        SoundManager.Instance?.StopBgm();
        SoundManager.Instance?.PlayGameOverBgm();

        // 하이라키에 태그 Enemy가 붙은 오브젝트 모두 제거
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Destroy(enemy);
        }
    }

    public void SpawnHitEffect(Vector3 pos)
    {
        if (hitEffectPrefab == null) return;
        Instantiate(hitEffectPrefab, pos, Quaternion.identity);
    }

    public void OnUltimateReadyChanged(bool isReady)
    {
        uiManager.SetUltimateReady(isReady);
    }

    public void ShowUltimateCutIn(bool show)
    {
        uiManager.ShowUltimateCutIn(show);
    }

    public void RestartGameWithButton()
    {
        SoundManager.Instance?.PlayUiClickSound();
        Invoke(nameof(RestartGame), 2.2f);
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

}
