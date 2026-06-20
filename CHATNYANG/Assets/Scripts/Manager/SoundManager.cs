using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum BgmType
{
    Title,
    GamePlay
}

// 1. 일반 효과음 Enum (UI, 피격, 획득 등)
public enum SfxType
{
    Hit,
    BtnClick,
    ExpPickup
}

// 2. 무기 전용 효과음 Enum으로 분리!
public enum WeaponSfxType
{
    Attack,
    LaserAttack,
    Slash
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM Settings")]
    public AudioSource bgmSource;
    public AudioClip[] bgmClips;

    [Header("General SFX Settings")]
    public AudioClip[] sfxClips;

    [Header("Weapon SFX Settings")]
    public AudioClip[] weaponSfxClips; // 무기 소리만 따로 넣을 배열

    [Header("Pool Settings")]
    public int sfxPoolSize = 15;
    private Queue<AudioSource> sfxPool;

    private float lastHitPlayTime = 0f;
    private readonly float hitSoundCooldown = 0.05f;

    [Header("Mixer Settings")]
    public AudioMixerGroup sfxMixerGroup;       // 일반 효과음 믹서
    public AudioMixerGroup weaponMixerGroup;    // 무기 효과음 믹서

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSfxPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSfxPool()
    {
        sfxPool = new Queue<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject obj = new GameObject("SfxSource");
            obj.transform.SetParent(transform);

            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxPool.Enqueue(source);
        }
    }

    public void PlayBGM(BgmType type)
    {
        int index = (int)type;
        if (index < 0 || index >= bgmClips.Length || bgmClips[index] == null) return;

        bgmSource.clip = bgmClips[index];
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 일반 효과음 재생 함수
    public void PlaySFX(SfxType type)
    {
        if (type == SfxType.Hit)
        {
            if (Time.time - lastHitPlayTime < hitSoundCooldown) return;
            lastHitPlayTime = Time.time;
        }

        int index = (int)type;
        if (index < 0 || index >= sfxClips.Length || sfxClips[index] == null) return;

        AudioSource source = sfxPool.Dequeue();
        source.clip = sfxClips[index];
        source.outputAudioMixerGroup = sfxMixerGroup; // 일반 믹서로 연결
        source.Play();

        sfxPool.Enqueue(source);
    }

    // 무기 전용 사운드 재생 함수 (새로 추가됨)
    public void PlayWeaponSFX(WeaponSfxType type)
    {
        int index = (int)type;
        if (index < 0 || index >= weaponSfxClips.Length || weaponSfxClips[index] == null) return;

        AudioSource source = sfxPool.Dequeue();
        source.clip = weaponSfxClips[index];

        // 무기 믹서가 할당되어 있으면 무기로, 아니면 기본 sfx 믹서로 안전하게 연결
        source.outputAudioMixerGroup = weaponMixerGroup != null ? weaponMixerGroup : sfxMixerGroup;
        source.Play();

        sfxPool.Enqueue(source);
    }
}