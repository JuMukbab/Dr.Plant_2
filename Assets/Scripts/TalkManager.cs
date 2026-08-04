using System.Collections;
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

    private void Awake()
    {
        Instance = this;
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

            if (audioSource != null
                && typingSound != null
                && character != ' '
                && character != '\n')
            {
                audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(typingSound, 0.35f);
            }

            if (character == '.' || character == '!' || character == '?')
                yield return new WaitForSeconds(0.25f);
            else if (character == ',')
                yield return new WaitForSeconds(0.15f);
            else
                yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }
}
