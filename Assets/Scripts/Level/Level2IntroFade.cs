using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Level2IntroFade : MonoBehaviour
{
    [SerializeField] private Image blackOverlay;

    private IEnumerator Start()
    {
        var pa = PersistentAudio.Instance;

        // Show black overlay
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            var c = blackOverlay.color;
            c.a = 1f;
            blackOverlay.color = c;
        }

        // Wait a tiny moment for scene to settle
        yield return null;

        // Fade in, synced with remaining music time
        float remaining = pa != null ? pa.RemainingTime() : 2f;
        if (remaining <= 0.5f) remaining = 2f;

        float elapsed = 0f;
        while (elapsed < remaining && blackOverlay != null)
        {
            elapsed += Time.deltaTime;
            var c = blackOverlay.color;
            c.a = 1f - Mathf.Lerp(0f, 1f, elapsed / remaining);
            blackOverlay.color = c;

            // Stop if music ended
            if (pa != null && !pa.IsPlaying()) break;
            yield return null;
        }

        // Fully clear
        if (blackOverlay != null)
        {
            var c = blackOverlay.color; c.a = 0f;
            blackOverlay.color = c;
            blackOverlay.gameObject.SetActive(false);
        }

        // Clean up persistent audio
        if (pa != null) pa.StopAndDestroy();
    }
}
