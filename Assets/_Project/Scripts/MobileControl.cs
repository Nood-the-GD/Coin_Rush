using UnityEngine;

public class MobileControl : MonoBehaviour
{
    void Awake()
    {
        gameObject.SetActive(Application.isMobilePlatform);
        if (Application.isMobilePlatform) Application.targetFrameRate = 60;
    }
}
