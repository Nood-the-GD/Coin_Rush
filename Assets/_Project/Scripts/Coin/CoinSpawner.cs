using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private NetworkObject _coinPrefab;
    [SerializeField] private int _maxCoins = 2;
    [SerializeField] private float _spawnInterval = 0.8f;
    [SerializeField] private Vector2 _areaHalfSize = new(7f, 3.5f);
    [SerializeField] private float _coinHeight = 1f;
    [SerializeField] private float _pickupRadius = 0.8f;

    private readonly List<NetworkObject> _coinNetworkList = new();
    private readonly List<Coin> _coinList = new();
    private bool _running;
    private float _timer;

    public void Begin()
    {
        _running = true;
        _timer = 0;
    }

    public void StopAndClear()
    {
        _running = false;
        foreach (var c in _coinNetworkList)
        {
            if (c != null && c.IsSpawned)
            {
                c.Despawn();
            }
        }
        _coinNetworkList.Clear();
        _coinList.Clear();
    }

    void Update()
    {
        var nm = NetworkManager.Singleton;
        if (!_running || nm == null || !nm.IsServer) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f && _coinNetworkList.Count < _maxCoins)
        {
            _timer = _spawnInterval;
            SpawnCoin();
        }

        CheckPickups();
    }

    private void CheckPickups()
    {
        float r2 = _pickupRadius * _pickupRadius;
        for(int i = _coinNetworkList.Count - 1; i >= 0; i--)
        {
            var coin = _coinNetworkList[i];
            foreach(var player in PlayerNetwork.All)
            {
                Vector3 distance = player.transform.position - coin.transform.position;
                distance.y = 0;

                if (distance.sqrMagnitude > r2) continue;

                player.Score.Value += _coinList[i].Value;
                player.PlayCoinSoundRpc();
                coin.Despawn();
                _coinNetworkList.RemoveAt(i);
                _coinList.RemoveAt(i);
                break;
            }
        }
    }

    private void SpawnCoin()
    {
        var pos = new Vector3(UnityEngine.Random.Range(-_areaHalfSize.x, _areaHalfSize.x), _coinHeight, UnityEngine.Random.Range(-_areaHalfSize.y, _areaHalfSize.y));
        var coinNetwork = Instantiate(_coinPrefab, pos, _coinPrefab.transform.rotation);
        bool isBigCoin = GameManager.Instance.TimeLeft() < 5;
        var coin  = coinNetwork.GetComponent<Coin>();
        coin.IsBigCoin.Value = isBigCoin;
        coinNetwork.Spawn();
        _coinNetworkList.Add(coinNetwork);
        _coinList.Add(coin);
    }
}
