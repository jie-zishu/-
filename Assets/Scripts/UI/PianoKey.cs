using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class PianoKey : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AudioClip clip;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (clip != null) audioSource.clip = clip;
    }

    public void PlayNote()
    {
        if (audioSource != null && audioSource.clip != null)
            audioSource.PlayOneShot(audioSource.clip);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayNote();
    }
}
