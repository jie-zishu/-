using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

[RequireComponent(typeof(PlayableDirector))]
public class Level1EndingDirector : MonoBehaviour
{
    [SerializeField] private Image blackOverlay;
    [SerializeField] private AudioClip endingMusic;
    [SerializeField] private float fadeDuration = 1.5f;

    private PlayableDirector director;
    private bool hasPlayed;
    private PersistentAudio persistentAudio;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        director.stopped += OnTimelineStopped;
        if (blackOverlay != null) blackOverlay.gameObject.SetActive(false);
    }

    public void PlayEnding()
    {
        if (hasPlayed) return;
        hasPlayed = true;

        // Create persistent audio that survives scene transitions
        var audioGO = new GameObject("PersistentEndingMusic");
        audioGO.AddComponent<AudioSource>();
        persistentAudio = audioGO.AddComponent<PersistentAudio>();
        persistentAudio.PlayClip(endingMusic);

        director.Play();
    }

    /// <summary>
    /// Called by Timeline Signal to show MP3 screen emission effect.
    /// </summary>
    public void ShowMP3ScreenEmission()
    {
        var screenGO = GameObject.Find("ForMP3/MP3/MP3 Model/ScreenEmission");
        if (screenGO != null) screenGO.SetActive(true);
    }

    /// <summary>
    /// Called by Timeline Signal at the "fade to black" moment.
    /// </summary>
    public void TriggerFadeToBlack()
    {
        StartCoroutine(FadeOutAndLoadLevel2());
    }

    private IEnumerator FadeOutAndLoadLevel2()
    {
        // Move overlay to Canvas root so it survives piano UI closing
        if (blackOverlay != null)
        {
            var canvas = blackOverlay.GetComponentInParent<Canvas>();
            if (canvas != null) blackOverlay.transform.SetParent(canvas.transform, false);
            blackOverlay.transform.SetAsLastSibling();
            // Reset to full screen
            var rt = blackOverlay.GetComponent<RectTransform>();
            if (rt != null) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }
            blackOverlay.transform.localScale = Vector3.one;
            blackOverlay.gameObject.SetActive(true);
        }
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (blackOverlay != null)
            {
                var c = blackOverlay.color;
                c.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                blackOverlay.color = c;
            }
            yield return null;
        }
        if (blackOverlay != null) { var c = blackOverlay.color; c.a = 1f; blackOverlay.color = c; }

        // Load Level2 — persistentAudio survives because of DontDestroyOnLoad
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level2");
    }

    private void OnTimelineStopped(PlayableDirector d)
    {
        // If Signal wasn't placed, auto-fade anyway
        if (persistentAudio != null && !persistentAudio.IsPlaying())
            StartCoroutine(FadeOutAndLoadLevel2());
    }

    private void OnDestroy()
    {
        if (director != null) director.stopped -= OnTimelineStopped;
    }
}
