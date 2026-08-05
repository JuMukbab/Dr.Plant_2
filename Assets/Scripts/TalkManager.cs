using UnityEngine;
using TMPro;
using System.Collections;

public class TalkManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip typingSound;
    public float typingSpeed = 0.03f;

    Coroutine typingCoroutine;
    public static TalkManager Instance;

    public TMP_Text talkText;

    void Awake()
    {
        Instance = this;
    }
    IEnumerator TypeText(string text)
    {
        talkText.text = "";

        foreach (char c in text)
        {
            talkText.text += c;

            // 효과음이 등록되어 있을 때만 재생
            if (audioSource != null &&
                typingSound != null &&
                c != ' ' &&
                c != '\n')
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(typingSound, 0.35f);
            }

            if (c == '.' || c == '!' || c == '?')
                yield return new WaitForSeconds(0.25f);

            else if (c == ',')
                yield return new WaitForSeconds(0.15f);

            else
                yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }
    public void Show(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));
    }
}
