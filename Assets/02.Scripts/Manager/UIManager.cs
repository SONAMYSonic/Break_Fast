using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public void UpdateHp(int current, int max)
    {
        if (hpSlider == null) return;
        hpSlider.maxValue = max;
        hpSlider.value = current;
    }

    public void UpdateScore(int score, int bestScore)
    {
        if (scoreText != null) scoreText.text = $"Current: {score}";
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
            // TODO: 애니메이션(Scale 튕기기)
        }
    }

    public void ShowGameOver(int score, int bestScore)
    {
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);
        if (gameOverScoreText != null) gameOverScoreText.text = $"SCORE {score}";
        if (gameOverBestScoreText != null) gameOverBestScoreText.text = $"BEST {bestScore}";
    }

    public void PopupScore(int addScore)
    {
        if (popupScoreText == null) return;

        popupScoreText.text = $"+{addScore}!";
        // TODO: 알파/위치 애니메이션 코루틴으로
    }
}
