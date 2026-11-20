using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] private Image _emojiImage;
    [SerializeField] private Image _dashCooldown;
    [SerializeField] private GameObject _timer;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Sprite[] _emojiArray;
    [SerializeField] private InputActionReference _emojiActionReference1, _emojiActionReference2, _emojiActionReference3;
    private InputAction _emojiAction1, _emojiAction2, _emojiAction3;
    private float _emojiDuration = 2f;
    private float _clientEmojiTriggerTime;
    private float _serverEmojiTriggerTime;
    private float _emojiCoolDown = 3f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner || !IsSpawned) return;

        _emojiAction1 = _emojiActionReference1.action;
        _emojiAction2 = _emojiActionReference2.action;
        _emojiAction3 = _emojiActionReference3.action;
        _emojiAction1.Enable();
        _emojiAction2.Enable();
        _emojiAction3.Enable();
    }

    public override void OnNetworkPreDespawn()
    {
        if (!IsOwner) return;

        _emojiAction1.Disable();
        _emojiAction2.Disable();
        _emojiAction3.Disable();
    }

    void Update()
    {
        // Run in all machine, avoid show but no hide
        if(Time.time - _clientEmojiTriggerTime > _emojiDuration)
        {
            _emojiImage.sprite = null;
            _emojiImage.enabled = false;
        }

        if (!IsOwner)
        {
            _timer.SetActive(false);
            return;

        }

        if (_emojiAction1.WasPressedThisFrame())
        {
            ShowEmojiRpc(0);
        }
        if(_emojiAction2.WasPressedThisFrame())
        {
            ShowEmojiRpc(1);
        }
        if(_emojiAction3.WasPressedThisFrame())
        {
            ShowEmojiRpc(2);
        }

        _dashCooldown.fillAmount = _playerMovement.DashCoolDownRatio;
    }

    [Rpc(SendTo.Server)]
    private void ShowEmojiRpc(int index)
    {
        if(Time.time - _serverEmojiTriggerTime > _emojiCoolDown)
        {
            _serverEmojiTriggerTime = Time.time;
            ShowEmojiClientAndHostRpc(index);
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowEmojiClientAndHostRpc(int index)
    {
        var i = index % _emojiArray.Length;

        _clientEmojiTriggerTime = Time.time;
        _emojiImage.sprite = _emojiArray[i];
        _emojiImage.enabled = true;
    }
}
