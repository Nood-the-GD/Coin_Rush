using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [Header("Pre Game")]
    [SerializeField] private RelayConnector _relay;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private TMP_InputField _nameInput, _codeInput;
    [SerializeField] private Button _hostButton, _joinButton;
    [SerializeField] private TextMeshProUGUI _statusText, _roomCodeText;

    void Start()
    {
        _nameInput.text = PlayerPrefs.GetString("playerName", "");
        _hostButton.onClick.AddListener(OnHost);
        _joinButton.onClick.AddListener(OnJoin);
        NetworkManager.Singleton.OnClientStopped += (b) =>
        {
            var reason = NetworkManager.Singleton.DisconnectReason;
            ShowMenu(string.IsNullOrEmpty(reason) ? reason : "Disconnected");
        };
    }

    private async void OnHost()
    {
        SaveName();
        SetBusy(true, "Creating room...");
        try
        {
            string code = await _relay.HostAsync(4);
            if(code == null)
            {
                ShowMenu("Can't create room");
                return;
            }
            _roomCodeText.text = $"Code: {code}";
            _menuPanel.SetActive(false);
        }
        catch (Exception e)
        {
            ShowMenu("Error: " + e.Message);
        }
    }

    private async void OnJoin()
    {
        SaveName();
        SetBusy(true, "Joining...");

        try
        {
            bool ok = await _relay.JoinAsync(_codeInput.text);
            if(!ok )
            {
                ShowMenu("Can't join");
                return;
            }
            _roomCodeText.text = $"Room code: {_codeInput.text.ToUpperInvariant()}";
            _menuPanel.SetActive(false);
        }
        catch(Exception e)
        {
            ShowMenu("Wrong code or network error: " + e.Message);
        }
    }

    private void ShowMenu(string msg)
    {
        _menuPanel.SetActive(true);
        _roomCodeText.text = "";
        SetBusy(false, msg);
    }

    private void SaveName()
    {
        PlayerPrefs.SetString("playerName", _nameInput.text);
    }

    private void SetBusy(bool isBusy, string msg)
    {
        _hostButton.interactable = _joinButton.interactable = !isBusy;
        _statusText.text = msg;
    }
}
