using UnityEngine;
using Unity.Netcode;
using System;


public class PlayerNetwork : NetworkBehaviour{

    public override void OnNetworkSpawn()
    {
        gameObject.name = $"Player_{OwnerClientId}";
        Debug.Log($"Player spawn {name} | IsOwner: {IsOwner} | IsServer: {IsServer}");

        if (IsOwner)
        {
            GetComponent<Renderer>().material.color = Color.green;
        }
    }

    public override void OnNetworkDespawn()
    {
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
