using UnityEngine;

public class RotateToCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;
    }
}
