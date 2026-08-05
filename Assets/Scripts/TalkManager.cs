using System.Collections;
using System.Collections.Generic;
using DrPlant.Data;
using TMPro;
using UnityEngine;

public class TalkManager : MonoBehaviour
{
    public static TalkManager Instance;

    public AudioSource audioSource;
    public AudioClip typingSound;
    public float typingSpeed = 0.03f;
    public TMP_Text talkText;

    private Coroutine typingCoroutine;
    private readonly Dictionary<PatientId, AudioClip> voiceClips =
        new Dictionary<PatientId, AudioClip>();
    private float lastTypingSoundTime = float.NegativeInfinity;

    private const float MasterTypingVolume = 2.4f;
    private const float TypingSoundInterval = 0.05f;

    private void Awake()
    {
        Instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        foreach (AudioClip clip in voiceClips.Values)
            Destroy(clip);

        voiceClips.Clear();
    }

    public void Show(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Clear();
            return;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    public void Clear()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (talkText != null)
            talkText.text = string.Empty;
    }

    private IEnumerator TypeText(string text)
    {
        if (talkText == null)
        {
            typingCoroutine = null;
            yield break;
        }

        talkText.text = string.Empty;

        foreach (char character in text)
        {
            talkText.text += character;

            PlayTypingSound(character);

            if (character == '.' || character == '!' || character == '?')
                yield return new WaitForSeconds(0.25f);
            else if (character == ',')
                yield return new WaitForSeconds(0.15f);
            else
                yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }

    private void PlayTypingSound(char character)
    {
        if (audioSource == null || ShouldSkipTypingSound(character))
            return;

        if (Time.unscaledTime - lastTypingSoundTime < TypingSoundInterval)
            return;

        PatientDefinition patient =
            PatientManager.Instance?.ActiveCase?.Patient;
        PatientVoiceProfile voice = patient?.Voice;

        if (patient == null || voice == null)
        {
            PlayFallbackTypingSound();
            return;
        }

        if (!voiceClips.TryGetValue(patient.Id, out AudioClip clip))
        {
            clip = CreateVoiceClip(patient.Id, voice);
            voiceClips.Add(patient.Id, clip);
        }

        float pitchOffset = UnityEngine.Random.Range(
            -voice.PitchVariation,
            voice.PitchVariation);
        audioSource.pitch = Mathf.Max(80f, voice.Frequency + pitchOffset)
            / voice.Frequency;
        audioSource.PlayOneShot(clip, voice.Volume * MasterTypingVolume);
        lastTypingSoundTime = Time.unscaledTime;
    }

    private void PlayFallbackTypingSound()
    {
        if (typingSound == null)
            return;

        audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(typingSound, 0.35f);
        lastTypingSoundTime = Time.unscaledTime;
    }

    private static bool ShouldSkipTypingSound(char character)
    {
        return char.IsWhiteSpace(character)
            || character == '.'
            || character == ','
            || character == '!'
            || character == '?'
            || character == '…';
    }

    private static AudioClip CreateVoiceClip(
        PatientId patientId,
        PatientVoiceProfile voice)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        if (sampleRate <= 0)
            sampleRate = 44100;

        int sampleCount = Mathf.Max(
            1,
            Mathf.CeilToInt(sampleRate * voice.Duration));
        float[] samples = new float[sampleCount];
        float angularFrequency = 2f * Mathf.PI * voice.Frequency;

        for (int index = 0; index < sampleCount; index++)
        {
            float time = index / (float)sampleRate;
            float phase = angularFrequency * time;
            float waveform = EvaluateWaveform(voice.Waveform, phase);
            float attack = Mathf.Clamp01(time / 0.002f);
            float decay = Mathf.Exp(-7f * time / voice.Duration);
            samples[index] = waveform * attack * decay;
        }

        AudioClip clip = AudioClip.Create(
            $"DrPlant_{patientId}_Voice",
            sampleCount,
            1,
            sampleRate,
            false);
        clip.hideFlags = HideFlags.DontSave;
        clip.SetData(samples, 0);
        return clip;
    }

    private static float EvaluateWaveform(
        PatientVoiceWaveform waveform,
        float phase)
    {
        float sine = Mathf.Sin(phase);

        switch (waveform)
        {
            case PatientVoiceWaveform.Square:
                return sine >= 0f ? 1f : -1f;
            case PatientVoiceWaveform.Triangle:
                return 2f / Mathf.PI * Mathf.Asin(sine);
            default:
                return sine;
        }
    }
}
