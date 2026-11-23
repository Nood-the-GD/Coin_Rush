using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public ISession ActiveSession { get; private set; }

    private Task _initAsync;
    private async Task InitUnityService() 
    {
        await UnityServices.InitializeAsync(); 
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Awake() 
    {
        _initAsync = InitUnityService();
    }

    async void Start()
    {
        await _initAsync;
        NetworkManager.Singleton.OnClientDisconnectCallback += async (clientId) =>
        {
            if (clientId != NetworkManager.Singleton.LocalClientId) return;

            if(ActiveSession != null)
            {
                await Leave();
            }
        };
    }

    public async Task CreateSession()
    {
        await _initAsync;
        var option = new SessionOptions
        {
            Name = "Coin Rush Room",
            MaxPlayers = 4,
            IsPrivate = true
        }.WithRelayNetwork();

        ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(option);
        RegisterEvent(ActiveSession);
    }

    public async Task<bool> JoinByCode(string code)
    {
        await _initAsync;
        try
        {
            ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
            RegisterEvent(ActiveSession);
            return true;
        }
        catch (SessionException e)
        {
            switch (e.Error)
            {

                case SessionError.SessionNotFound:
                    Debug.LogWarning("Can't find room");
                    break;
                case SessionError.SessionDeleted:
                    Debug.LogWarning("Room deleted");
                    break;
                case SessionError.RateLimitExceeded:
                    Debug.LogWarning("Too many request, try later");
                    break;
                case SessionError.NetworkManagerStartFailed:
                    Debug.LogWarning("Fail to connect room");
                    break;
                default:
                    Debug.LogError($"Join fail: {e.Error} - {e.Message}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Join Fail: {e.Message}");
        }
        return false;
    }

    public async Task QuickJoin()
    {
        await _initAsync;
        var quickJoinOptions = new QuickJoinOptions { CreateSession = true };
        var sessionOptions = new SessionOptions
        {
            MaxPlayers = 4,
        }.WithRelayNetwork();

        ActiveSession = await MultiplayerService.Instance.MatchmakeSessionAsync(quickJoinOptions, sessionOptions);
        RegisterEvent(ActiveSession);
    }

    public async Task ListSession()
    {
        await _initAsync;
        var result = await MultiplayerService.Instance.QuerySessionsAsync(new QuerySessionsOptions());
        foreach (var s in result.Sessions)
        {
            Debug.Log($"{s.Name} | {s.Id} | {s.MaxPlayers - s.AvailableSlots}/{s.MaxPlayers}");
        }
    }

    public async Task Leave()
    {
        if (ActiveSession == null) return;

        if (ActiveSession.IsHost) await ActiveSession.AsHost().DeleteAsync();
        else await ActiveSession.LeaveAsync();

        ActiveSession = null;
    }

    public async void LockSession(bool isLock)
    {
        await _initAsync;
        if (ActiveSession == null || !ActiveSession.IsHost) return;
        var host = ActiveSession.AsHost();
        host.IsLocked = isLock;
        await host.SavePropertiesAsync();
    }

    private void RegisterEvent(ISession session)
    {
        session.PlayerJoined += id => Debug.Log($"Player join: {id}");
        session.PlayerHasLeft += id => Debug.Log($"Player left: {id}");
        session.Deleted += () =>
        {
            Debug.Log("Session Deleted");
            ActiveSession = null;
        };
        session.RemovedFromSession += () =>
        {
            Debug.Log("Removed from session");
            ActiveSession = null;
        };
    }
}
