using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverBestScoreText;
    [SerializeField] private TextMeshProUGUI popupScoreText;

    [Header("Ultimate UI")]
    [SerializeField] private GameObject ultimateReadyHint;   // "SPACE" 아이콘/텍스트
    [SerializeField] private GameObject ultimateCutInPanel;  // 캐릭터/움짤 패널

    public void UpdateHp(int current, int max)
    {
        if (hpSlider == null) return;
        hpSlider.maxValue = max;
        hpSlider.value = current;
    }

    public void UpdateScore(int score, int bestScore)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (bestScoreText != null) bestScoreText.text = $"BEST {bestScore}";
    }

    public void UpdateCombo(int combo)
    {
        if (comboText == null) return;

        if (combo <= 0)
        {
            comboText.gameObject.SetActive(false);
        }
        else
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"COMBO! x{combo}";
            comboText.transform.DOKill(true); // 이전 애니메이션이 있으면 제거
            comboText.transform.DOPunchScale(Vector3.one * 2f, 0.2f, 3, 1);
            // TODO: 애니메이션(Scale 튕기기)
        }
    }

    public void ShowGameOver(int score, int bestScore)
    {
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);
        if (gameOverScoreText != null) gameOverScoreText.text = $"SCORE {score:N0}";
        if (gameOverBestScoreText != null) gameOverBestScoreText.text = $"BEST {bestScore}";
    }

    public void PopupScore(int addScore)
    {
        if (popupScoreText == null) return;

        popupScoreText.text = $"+{addScore}!";
        // TODO: 알파/위치 애니메이션 코루틴으로
    }

    public void SetUltimateReady(bool ready)
    {
        if (ultimateReadyHint == null) return;
        ultimateReadyHint.SetActive(ready);
    }

    public void ShowUltimateCutIn(bool show)
    {
        if (ultimateCutInPanel == null) return;
        ultimateCutInPanel.SetActive(show);
        if (show)
        {
            ultimateCutInPanel.transform.DOKill(true);
            ultimateCutInPanel.transform.DOShakePosition(0.5f, strength: 20f, vibrato: 50, randomness: 90);
        }
    }
}
