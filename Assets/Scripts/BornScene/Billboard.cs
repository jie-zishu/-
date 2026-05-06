using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }
}
