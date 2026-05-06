using UnityEngine;
using UnityEngine.Events;

public class TriggerDetector : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";

    public UnityEvent<GameObject> onTriggerEnter;
    public UnityEvent<GameObject> onTriggerExit;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
            onTriggerEnter?.Invoke(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
            onTriggerExit?.Invoke(other.gameObject);
    }
}
