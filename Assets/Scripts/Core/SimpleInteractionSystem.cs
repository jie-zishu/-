using UnityEngine;

public class SimpleInteractionSystem : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;

    private void Update()
    {
        // 检查F键按下
        if (Input.GetKeyDown(interactionKey))
        {
            // 检查前方是否有可交互物体
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, interactionDistance))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (hitObject.CompareTag("Interactable"))
                {
                    // 触发交互
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