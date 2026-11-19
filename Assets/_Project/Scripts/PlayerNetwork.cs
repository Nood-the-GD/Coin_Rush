using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using Unity.Collections;
using TMPro;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine.InputSystem;


public class PlayerNetwork : NetworkBehaviour
{
    public static readonly List<PlayerNetwork> All = new();

    public NetworkVariable<FixedString64Bytes> PlayerName = new();
    public NetworkVariable<int> ColorIndex = new();
    public NetworkVariable<int> Score = new();

    [SerializeField] private Renderer _body;
    [SerializeField] private TMP_Text _nameLabel;
    [SerializeField] private Color[] _palette = { Color.red, Color.cyan, Color.yellow, Color.magenta };

    public override void OnNetworkSpawn()
    {
        All.Add(this);
        PlayerName.OnValueChanged += OnNameChanged;
        ColorIndex.OnValueChanged += OnColorIndexChanged;
        Score.OnValueChanged += OnScoreChanged;

        OnNameChanged(default, PlayerName.Value);
        OnColorIndexChanged(0, ColorIndex.Value);
        OnScoreChanged(0, Score.Value);

        if (IsServer) ColorIndex.Value = FindFreeColor((int)OwnerClientId);
        if (IsOwner) SubmitNameRpc(PlayerPrefs.GetString("playerName", $"Player_{OwnerClientId}"));
    }

    private void OnScoreChanged(int previousValue, int newValue)
    {
        _nameLabel.text = $"{PlayerName.Value.ToString()} ({newValue})";
    }

    private void OnColorIndexChanged(int previousValue, int newValue)
    {
        // Protection layer, recalculate newValue
        _body.material.color = _palette[newValue % _palette.Length];
    }

    private void OnNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        _nameLabel.text = $"{newValue} ({Score.Value})";
    }

    public override void OnNetworkDespawn()
    {
        All.Remove(this);
        PlayerName.OnValueChanged -= OnNameChanged;
        Score.OnValueChanged -= OnScoreChanged;
        ColorIndex.OnValueChanged -= OnColorIndexChanged;
    }

    [Rpc(SendTo.Server)]
    private void SubmitNameRpc(string name)
    {
        string clean = string.IsNullOrWhiteSpace(name) ? $"Player{OwnerClientId}" : name.Trim();
        if (clean.Length > 12) clean = clean.Substring(0, 12);
        PlayerName.Value = clean;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (IsOwner)
        {
            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                RequestChangeColorRpc();
            }
        }

        if (!IsServer) return;

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Score.Value++;
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestChangeColorRpc()
    {
        ColorIndex.Value = FindFreeColor(ColorIndex.Value + 1);
    }

    private int FindFreeColor(int start)
    {
        var temp = start;
        temp = (temp + 1) % _palette.Length;
        for (int i = 0; i < _palette.Length; i++)
        {
            int c = (start + i) % _palette.Length;
            if (!All.Any(x => x.ColorIndex.Value == c && x != this)) return c;
        }
        return ColorIndex.Value;
    }


    // Leave this here to know how to code
    // Server/Host: ClientConnected / ClientDisconnected for every client.
    // Client: ClientConnected/Disconnected only for itself, PeerConnected/PeerDisconnected for other clients.
    private void OnConnectionEvent(NetworkManager nm, ConnectionEventData data)
    {
        switch (data.EventType)
        {
            case ConnectionEvent.ClientConnected:
                Debug.Log($"[{name}] ClientConnected: {data.ClientId}");
                break;
            case ConnectionEvent.ClientDisconnected:
                Debug.Log($"[{name}] ClientDisconnected: {data.ClientId}");
                break;
            case ConnectionEvent.PeerConnected:
                Debug.Log($"[{name}] PeerConnected: {data.ClientId}");
                break;
            case ConnectionEvent.PeerDisconnected:
                Debug.Log($"[{name}] PeerDisconnected: {data.ClientId}");
                break;
        }
    }
}
