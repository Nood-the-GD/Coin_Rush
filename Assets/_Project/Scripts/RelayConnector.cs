using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class RelayConnector : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    const string CONNECTION_TYPE = "wss";
#else
    const string CONNECTION_TYPE = "dtls";
#endif

    private async Task EnsureSignedInAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    UnityTransport Transport => NetworkManager.Singleton.GetComponent<UnityTransport>();

    public async Task<string> HostAsync(int maxPlayers = 0)
    {
        await EnsureSignedInAsync();
        var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);

        Transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, CONNECTION_TYPE));
        Transport.UseWebSockets = CONNECTION_TYPE == "wss";
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    public async Task<bool> JoinAsync(string joinCode)
    {
        await EnsureSignedInAsync();
        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode.Trim().ToUpperInvariant());

        Transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, CONNECTION_TYPE));
        Transport.UseWebSockets = CONNECTION_TYPE == "wss";
        return NetworkManager.Singleton.StartClient();
    }
}
