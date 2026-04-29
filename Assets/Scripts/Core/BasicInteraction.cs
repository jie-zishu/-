using UnityEngine;

public class BasicInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;

    private void Update()
    {
        if (Input.GetKeyDown(interactionKey))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, interactionDistance))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (hitObject.CompareTag("Interactable"))
                {
                    InteractableObject interactable = hitObject.GetComponent<InteractableObject>();
                    if (interactable != null)
                    {
                        interactable.Interact();
                    }
                }
            }
        }
    }
}