using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] public int levelIndex;

    private bool isHighlighted;
    private Color defaultColor = Color.red;
    private Renderer cachedRenderer;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        if (cachedRenderer != null && cachedRenderer.sharedMaterial != null)
        {
            defaultColor = cachedRenderer.sharedMaterial.color;
        }
    }

    public void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;

        if (cachedRenderer == null)
        {
            cachedRenderer = GetComponent<Renderer>();
        }

        if (cachedRenderer != null)
        {
            cachedRenderer.material.color = highlight ? Color.yellow : defaultColor;
        }
    }

    public void Interact()
    {
        if (GameFrameworkManager.Instance != null)
        {
            GameFrameworkManager.Instance.CompleteLevel(levelIndex);
        }
    }

    public bool IsHighlighted()
    {
        return isHighlighted;
    }
}
