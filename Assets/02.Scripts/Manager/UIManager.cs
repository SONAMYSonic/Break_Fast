using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _bestScoreText;
    [SerializeField] private TextMeshProUGUI _comboText;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _gameOverShutter;
    [SerializeField] private TextMeshProUGUI _gameOverScoreText;
    [SerializeField] private TextMeshProUGUI _gameOverBestScoreText;
    [SerializeField] private TextMeshProUGUI _popupScoreText;
    public GameObject GameOverScore;
    public GameObject GameOverBestScore;
    public GameObject GameOverPostIT;
    public GameObject RestartButton;
    public GameObject TitleButton;
    public GameObject KeyGuide;

    [Header("Ultimate UI")]
    [SerializeField] private GameObject ultimateReadyHint;   // "SPACE" 아이콘/텍스트
    [SerializeField] private GameObject ultimateCutInPanel;  // 캐릭터/움짤 패널

    [Header("Game Start UI")]
    [SerializeField] private Image fadeImage;                // 풀스크린 검은 이미지
    [SerializeField] private TextMeshProUGUI countdownText;  // 3 2 1 GO 텍스트

    public void UpdateHp(int current, int max)
    {
        if (_hpSlider == null) return;
        _hpSlider.maxValue = max;
        _hpSlider.value = current;
    }

    public void UpdateScore(int score, int bestScore)
    {
        if (_scoreText != null) _scoreText.text = $"Score: {score:N0}";
        if (_bestScoreText != null) _bestScoreText.text = $"BEST {bestScore:N0}";
    }

    public void UpdateCombo(int combo)
    {
        if (_comboText == null) return;

        if (combo <= 0)
        {
            _comboText.gameObject.SetActive(false);
        }
        else
        {
            _comboText.gameObject.SetActive(true);
            _comboText.text = $"COMBO! x{combo}";
            _comboText.transform.DOKill(true); // 이전 애니메이션이 있으면 제거
            _comboText.transform.DOPunchScale(Vector3.one * 2f, 0.2f, 3, 1);
            // TODO: 애니메이션(Scale 튕기기)
        }
    }

    public void ShowGameOver(int score, int bestScore)
    {
        _gameOverPanel.SetActive(true);
        _gameOverPanel.transform.DOScale(Vector3.one, 0.2f);
        _gameOverShutter.SetActive(true);
        _gameOverShutter.transform.DOLocalMoveY(0f, 1f);
        Invoke("GameOverTexts", 1f);
        _gameOverScoreText.text = $"SCORE {score:N0}";
        _gameOverBestScoreText.text = $"BEST {bestScore:N0}";
    }

    public void GameOverTexts()
    {
        GameOverScore.SetActive(true);
        GameOverBestScore.SetActive(true);
        GameOverPostIT.SetActive(true);
        RestartButton.SetActive(true);
        TitleButton.SetActive(true);
    }

    public void PopupScore(int addScore)
    {
        if (_popupScoreText == null) return;

        _popupScoreText.text = $"+{addScore}!";
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
            ultimateCutInPanel.transform.DOShakePosition(0.3f, strength: 30f, vibrato: 60, randomness: 90);
        }
    }

    public void SetFadeActive(bool active)
    {
        if (fadeImage == null) return;
        fadeImage.gameObject.SetActive(active);
    }

    public void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null) return;
        var c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    public void SetCountdownText(string text)
    {
        if (countdownText == null) return;

        if (string.IsNullOrEmpty(text))
        {
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = text;
        }
    }

    public void SetKeyGuideActive(bool active)
    {
        if (KeyGuide == null) return;
        KeyGuide.SetActive(active);
    }
}
