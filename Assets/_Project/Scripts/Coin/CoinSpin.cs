using UnityEngine;

public class CoinSpin : MonoBehaviour
{
    [SerializeField] private float _degreePerSecond = 180;

    void Update()
    {
        transform.Rotate(0f, _degreePerSecond * Time.deltaTime, 0f, Space.World);        
    }
}
