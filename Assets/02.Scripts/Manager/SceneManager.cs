using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public float _titleDelay = 6.0f;
    public GameObject _fadeObject;
    public Image _fadeImage;
    private float _timer = 0.0f;
    private float _fadeDuration = 6.0f;
    private bool _isPlaying = false;

    private void Update()
    {
        // 페이드 아웃, _fadeDuration 동안 _fadeImage의 알파 값을 0에서 1로 변경
        if (_isPlaying)
        {
            _timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(_timer / _fadeDuration);
            Color color = _fadeImage.color;
            color.a = alpha;
            _fadeImage.color = color;
        }
    }

    // 버튼 클릭 시 6.5초 후 "GameScene" 씬으로 전환
    public void OnStartButtonDelayed()
    {
        Invoke("OnStartButtonClicked", _titleDelay);
    }


    public void OnStartButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    // 버튼 클릭 시 "Title" 씬으로 전환
    public void OnBackToTitleButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void ScreenFadeOut()
    {
        _isPlaying = true;
        _fadeObject.SetActive(true);
    }
}
