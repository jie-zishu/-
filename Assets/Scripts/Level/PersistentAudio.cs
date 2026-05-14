using UnityEngine;

/// <summary>
/// Attach to a GameObject with AudioSource. Survives scene loads.
/// Level2 can stop/fade it when ready.
/// </summary>
public class PersistentAudio : MonoBehaviour
{
    public static PersistentAudio Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public float RemainingTime()
    {
        if (audioSource == null || audioSource.clip == null) return 0f;
        return audioSource.clip.length - audioSource.time;
    }

    public bool IsPlaying() => audioSource != null && audioSource.isPlaying;

    public void StopAndDestroy()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
