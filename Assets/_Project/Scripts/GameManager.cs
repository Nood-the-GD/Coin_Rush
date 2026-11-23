using System;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public enum GameState { Lobby, CountDown, Playing, Ended }

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float _roundLength = 60f;
    [SerializeField] private CoinSpawner _coinSpawner;
    [SerializeField] private int _countDownDuration = 3;
    private double _timeToStart;

    public NetworkVariable<GameState> State = new(GameState.Lobby);
    public NetworkVariable<double> EndTime = new();
    public NetworkVariable<int> CountDownTime = new();

    void Awake()
    {
        Instance = this;
        NetworkManager.Singleton.ConnectionApprovalCallback = OnConnectionApproval;
    }

    private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (State.Value == GameState.Playing)
        {
            response.Approved = false;
            response.Reason = "Room Playing";
        }
        else
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }

    }

    public float TimeLeft()
    {
        return State.Value == GameState.Playing ? Mathf.Max(0f, (float)(EndTime.Value - NetworkManager.ServerTime.Time)) : 0f;
    }

    [Rpc(SendTo.Server)]
    public void RequestStartRpc(RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
        if (State.Value == GameState.Playing) return;

        _timeToStart = NetworkManager.ServerTime.Time + _countDownDuration;
        State.Value = GameState.CountDown;
    }

    void Update()
    {
        if (!IsServer) return;

        if (State.Value != GameState.CountDown && State.Value != GameState.Playing) return;

        if(State.Value == GameState.CountDown)
        {
            var temp = Mathf.CeilToInt((float)_timeToStart - (float)NetworkManager.ServerTime.Time);
            if(CountDownTime.Value != temp)
            {
                CountDownTime.Value = Mathf.CeilToInt((float)_timeToStart -(float) NetworkManager.ServerTime.Time);
            }
        }

        if(NetworkManager.ServerTime.Time >= _timeToStart && State.Value != GameState.Playing)
        {
            foreach (var p in PlayerNetwork.All) p.Score.Value = 0;
            EndTime.Value = NetworkManager.ServerTime.Time + _roundLength;
            State.Value = GameState.Playing;
            _coinSpawner.Begin();
        }


        if(NetworkManager.ServerTime.Time >= EndTime.Value && State.Value == GameState.Playing)
        {
            State.Value = GameState.Ended;
            _coinSpawner.StopAndClear();
        }
    }
}
