using UnityEngine;

public class FloatOscillator : MonoBehaviour
{
    [SerializeField] private float minY = 1.1f;
    [SerializeField] private float maxY = 1.15f;
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float riseDuration = 1.5f;

    private bool isFloating;
    private float startY;
    private float elapsed;

    public void StartFloating()
    {
        if (isFloating) return;
        isFloating = true;
        startY = transform.position.y;
        elapsed = 0f;
    }

    private void Update()
    {
        if (!isFloating) return;

        elapsed += Time.deltaTime;

        if (elapsed < riseDuration)
        {
            float targetY = Mathf.Lerp(startY, (minY + maxY) * 0.5f, elapsed / riseDuration);
            Vector3 pos = transform.position;
            pos.y = targetY;
            transform.position = pos;
        }
        else
        {
            float mid = (minY + maxY) * 0.5f;
            float amp = (maxY - minY) * 0.5f;
            Vector3 pos = transform.position;
            pos.y = mid + Mathf.Sin((elapsed - riseDuration) * speed) * amp;
            transform.position = pos;
        }
    }
}
