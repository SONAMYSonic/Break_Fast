using System;
using System.Collections.Generic;
using UnityEngine;

public enum SfxType
{
    CarDash,        // 플레이어 대쉬
    DashHit,        // 박치기 성공
    IdleHit,        // 가만히 있다가 들이받힘
    EnemyExplode,   // 적 파괴
    UiClick,        // UI 클릭
    GameOver,       // 게임오버
    GetItem,       // 아이템 획득
    UltimateVoice,  // 궁극기 보이스
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioClip[] _mainBgm;

    [Header("SFX Player")]
    [SerializeField] private AudioSource _sfxSource;  // 2D SFX

    [Header("필살기 사운드")]
    public AudioSource UltimateVoiceSource;
    public AudioClip[] UltimateVoiceClips;

    [Header("Countdown Voices")]
    [SerializeField] private AudioSource _countdownSource;
    [SerializeField] private AudioClip[] _countdownVoiceClips; // 3,2,1 용
    [SerializeField] private AudioClip[] _goVoiceClips;        // GO! 용
    private int _countdownIndex = 0;

    [Header("게임오버 BGM")]
    public AudioClip GameOverBgmSource;

    [Header("Ui 클릭")]
    public AudioClip UIClick;

    [Serializable]
    private struct SfxEntry
    {
        public SfxType Type;

        [Tooltip("이 상황에서 재생될 사운드 클립들(랜덤 재생)")]
        public AudioClip[] Clips;
    }

    [Header("SFX List (랜덤 재생)")]
    [SerializeField] private SfxEntry[] _sfxEntries;

    private readonly Dictionary<SfxType, AudioClip[]> _sfxTable
        = new Dictionary<SfxType, AudioClip[]>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 테이블 구성
        foreach (var entry in _sfxEntries)
        {
            if (entry.Clips == null || entry.Clips.Length == 0) continue;
            _sfxTable[entry.Type] = entry.Clips;
        }

        _countdownIndex = UnityEngine.Random.Range(0, _countdownVoiceClips.Length); // 랜덤 시작
    }

    // ---------------- BGM ----------------

    public void PlayBgm(bool loop = true)
    {
        if (_mainBgm == null || _bgmSource == null) return;

        _bgmSource.loop = loop;
        _bgmSource.clip = _mainBgm[UnityEngine.Random.Range(0, _mainBgm.Length)];
        _bgmSource.Play();
    }

    public void StopBgm()
    {
        _bgmSource?.Stop();
    }

    public void PlayGameOverBgm()
    {
        _bgmSource.clip = GameOverBgmSource;
        _bgmSource.volume = 0.5f;
        _bgmSource.Play();
    }

    // ---------------- SFX ----------------

    public void PlaySfx(SfxType type)
    {
        if (_sfxSource == null) return;
        if (!_sfxTable.TryGetValue(type, out var clips)) return;
        if (clips.Length == 0) return;

        var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        if (clip != null)
            _sfxSource.PlayOneShot(clip);
    }

    public void PlaySfxAt(SfxType type, Vector3 pos)
    {
        if (!_sfxTable.TryGetValue(type, out var clips)) return;
        if (clips.Length == 0) return;

        var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, pos);
    }

    public void PlayUltimateVoice()
    {
        if (UltimateVoiceSource == null || UltimateVoiceClips.Length == 0) return;
        var clip = UltimateVoiceClips[UnityEngine.Random.Range(0, UltimateVoiceClips.Length)];
        UltimateVoiceSource.PlayOneShot(clip);
    }

    public void PlayRandomCountdownVoice()
    {
        var clip = _countdownVoiceClips[_countdownIndex];
        if (clip != null)
            _countdownSource.PlayOneShot(clip);
    }

    public void PlayRandomGoVoice()
    {
        var clip = _goVoiceClips[_countdownIndex];
        if (clip != null)
            _countdownSource.PlayOneShot(clip);
    }

    public void PlayUiClickSound()
    {
        _sfxSource.volume = 1f;
        _sfxSource.PlayOneShot(UIClick);
    }

    public void PlayCarDash()
    {
        _sfxSource.clip = _sfxTable[SfxType.CarDash][0];
        _sfxSource.Play();
    }
}
