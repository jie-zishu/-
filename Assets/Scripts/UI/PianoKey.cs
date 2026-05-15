using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PianoKey : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AudioClip clip;

    public UnityEvent onKeyPressed;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (clip != null) audioSource.clip = clip;

        var texts = GetComponentsInChildren<Text>();
        foreach (var t in texts) t.raycastTarget = false;

        var img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
    }

    public void PlayNote()
    {
        if (audioSource != null && audioSource.clip != null)
            audioSource.PlayOneShot(audioSource.clip);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayNote();
        onKeyPressed?.Invoke();
    }
}
