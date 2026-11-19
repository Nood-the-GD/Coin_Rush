using System;
using Unity.Netcode;
using UnityEngine;

public class ConnectionLogger : MonoBehaviour
{
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong obj)
    {
        Debug.Log($"[{(NetworkManager.Singleton.IsServer ? "S" : "C")}] ClientDisconnected: {obj}");
    }

    private void OnClientConnected(ulong obj)
    {
        Debug.Log($"[{(NetworkManager.Singleton.IsServer ? "S" : "C")}] ClientConnected: {obj}");
    }
}
