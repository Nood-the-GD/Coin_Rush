using Unity.Netcode;
using UnityEngine;

public class NetworkDebugUI : MonoBehaviour
{
    string _name = "";

    void OnGUI()
    {
        var nm = NetworkManager.Singleton;
        GUILayout.BeginArea(new Rect(10, 10, 240, 200));
        if(!nm.IsClient && !nm.IsServer)
        {
            if (GUILayout.Button("Host")) nm.StartHost();
            if (GUILayout.Button("Client")) nm.StartClient();
            if (GUILayout.Button("Server")) nm.StartServer();
            _name = GUILayout.TextField(_name);
            if (GUILayout.Button("Set Name"))
            {
                PlayerPrefs.SetString("playerName", _name);
            }
        }
        else
        {
            string mode = nm.IsHost ? "Host" : nm.IsServer ? "Server" : "Client";
            GUILayout.Label($"Mode: {mode} | LocalClientID: {nm.LocalClientId}");
            if (GUILayout.Button("Shutdown")) nm.Shutdown();
        }
        GUILayout.EndArea();
    }
}
