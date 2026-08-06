using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ClinicAudioManager : MonoBehaviour
{
    public static ClinicAudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource effectsSource;

    [Header("Used by the latest web version")]
    [SerializeField] private AudioClip calmBgm;
    [SerializeField] private AudioClip gameStart;
    [SerializeField] private AudioClip purchase;
    [SerializeField] private AudioClip pop;

    [Header("Source assets reserved for later use")]
    [SerializeField] private AudioClip relaxingBgm;
    [SerializeField] private AudioClip speechSound;

    private Coroutine musicStartCoroutine;
    private bool clinicAudioStarted;

    public void Configure(
        AudioSource music,
        AudioSource effects,
        AudioClip calm,
        AudioClip start,
        AudioClip purchaseClip,
        AudioClip popClip,
        AudioClip relaxing,
        AudioClip speech)
    {
        musicSource = music;
        effectsSource = effects;
        calmBgm = calm;
        gameStart = start;
        purchase = purchaseClip;
        pop = popClip;
        relaxingBgm = relaxing;
        speechSound = speech;
        ConfigureSources();
    }

    private void Awake()
    {
        Instance = this;
        ConfigureSources();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void BeginClinicAudio()
    {
        if (clinicAudioStarted)
            return;

        clinicAudioStarted = true;
        PlayEffect(gameStart, 0.65f, 1f);

        if (musicStartCoroutine != null)
            StopCoroutine(musicStartCoroutine);

        musicStartCoroutine = StartCoroutine(StartMusicAfterDelay());
    }

    public void PlayPurchase()
    {
        PlayEffect(purchase, 0.20f, 1f);
    }

    public void PlayChecklistToggle(bool selected)
    {
        PlayEffect(pop, 0.50f, selected ? 1f : 0.82f);
    }

    private IEnumerator StartMusicAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.7f);

        if (musicSource != null && calmBgm != null)
        {
            musicSource.clip = calmBgm;
            musicSource.loop = true;
            musicSource.volume = 0.30f;
            musicSource.Play();
        }

        musicStartCoroutine = null;
    }

    private void PlayEffect(AudioClip clip, float volume, float pitch)
    {
        if (effectsSource == null || clip == null)
            return;

        effectsSource.pitch = pitch;
        effectsSource.PlayOneShot(clip, volume);
    }

    private void ConfigureSources()
    {
        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.30f;
        }

        if (effectsSource != null)
        {
            effectsSource.playOnAwake = false;
            effectsSource.loop = false;
            effectsSource.spatialBlend = 0f;
            effectsSource.volume = 1f;
        }
    }
}
