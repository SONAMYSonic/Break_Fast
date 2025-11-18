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
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioClip _mainBgm;

    [Header("SFX Player")]
    [SerializeField] private AudioSource _sfxSource;  // 2D SFX

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
    }

    private void Start()
    {
        PlayBgm();
    }

    // ---------------- BGM ----------------

    public void PlayBgm(bool loop = true)
    {
        if (_mainBgm == null || _bgmSource == null) return;

        _bgmSource.loop = loop;
        _bgmSource.clip = _mainBgm;
        _bgmSource.Play();
    }

    public void StopBgm()
    {
        _bgmSource?.Stop();
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
}
