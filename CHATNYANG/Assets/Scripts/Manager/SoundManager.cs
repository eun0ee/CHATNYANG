using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum BgmType
{
    Title,
    GamePlay
}

public enum SfxType
{
    Attack,
    LaserAttack,
    Hit,
    BtnClick
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM Settings")]
    public AudioSource bgmSource;
    public AudioClip[] bgmClips;

    [Header("SFX Settings")]
    public int sfxPoolSize = 15;
    public AudioClip[] sfxClips;

    private Queue<AudioSource> sfxPool;

    private float lastHitPlayTime = 0f;
    private readonly float hitSoundCooldown = 0.05f;

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

    [Header("Mixer Settings")]
    public AudioMixerGroup sfxMixerGroup;

    private void InitializeSfxPool()
    {
        sfxPool = new Queue<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject obj = new GameObject("SfxSource");
            obj.transform.SetParent(transform);

            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            // SFX 믹서 그룹 할당
            if (sfxMixerGroup != null)
            {
                source.outputAudioMixerGroup = sfxMixerGroup;
            }

            sfxPool.Enqueue(source);
        }
    }

    public void PlayBGM(BgmType type)
    {
        int index = (int)type;
        if (index < 0 || index >= bgmClips.Length || bgmClips[index] == null)
        {
            return;
        }

        bgmSource.clip = bgmClips[index];
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(SfxType type)
    {
        if (type == SfxType.Hit)
        {
            if (Time.time - lastHitPlayTime < hitSoundCooldown)
            {
                return;
            }
            lastHitPlayTime = Time.time;
        }

        int index = (int)type;
        if (index < 0 || index >= sfxClips.Length || sfxClips[index] == null)
        {
            return;
        }

        AudioSource source = sfxPool.Dequeue();
        source.clip = sfxClips[index];
        source.Play();

        sfxPool.Enqueue(source);
    }
}