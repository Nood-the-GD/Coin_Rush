using Unity.Netcode;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    public NetworkVariable<bool> IsBigCoin = new();
    [SerializeField] private GameObject _coinViewBig;
    [SerializeField] private GameObject _coinView;
    public int Value => IsBigCoin.Value ? 5 : 1;

    public override void OnNetworkSpawn()
    {
        SetCoinView(IsBigCoin.Value);
    }

    private void SetCoinView(bool isBig = false)
    {
        if (isBig)
        {
            _coinView.SetActive(false);
            _coinViewBig.SetActive(true);
        }
        else
        {
            _coinView.SetActive(true);
            _coinViewBig.SetActive(false);
        }
    }
}
