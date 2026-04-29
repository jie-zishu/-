using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;

    private InteractableObject currentInteractable;

    private void Update()
    {
        UpdateNearestInteractable();

        if (currentInteractable != null && Input.GetKeyDown(interactionKey))
        {
            currentInteractable.Interact();
        }
    }

    private void UpdateNearestInteractable()
    {
        InteractableObject nearest = null;
        float nearestDistance = float.MaxValue;

        InteractableObject[] allInteractables = FindObjectsOfType<InteractableObject>(false);
        Vector3 selfPosition = transform.position;

        foreach (InteractableObject candidate in allInteractables)
        {
            float distance = Vector3.Distance(selfPosition, candidate.transform.position);
            if (distance <= interactionDistance && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = candidate;
            }
        }

        if (currentInteractable != nearest)
        {
            if (currentInteractable != null)
            {
                currentInteractable.SetHighlight(false);
            }

            currentInteractable = nearest;

            if (currentInteractable != null)
            {
                currentInteractable.SetHighlight(true);
            }
        }
    }

    private void OnDisable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.SetHighlight(false);
            currentInteractable = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
