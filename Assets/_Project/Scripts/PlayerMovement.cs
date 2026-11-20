using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _speed = 6f;
    [SerializeField] private float _turnSpeed = 720f;
    [SerializeField] private float _dashSpeed = 18f;
    [SerializeField] private float _dashDuration = 0.15f;
    [SerializeField] private float _dashCooldown = 3f;
    [SerializeField] private Vector2 _arenaHalfSize = new Vector2(7.5f, 4f);
    [SerializeField] private ParticleSystem _dashFx;
    [SerializeField] private InputActionReference _dashActionReference;

    private InputAction _dashAction;
    private Vector3 _lastDirection = Vector3.forward;
    private float _dashUntil, _nextDashTime;

    [SerializeField] private InputActionReference _moveActionReference;
    private InputAction _moveAction;
    private Vector2 _input;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        _moveAction = _moveActionReference.action;
        _moveAction.Enable();
        _dashAction = _dashActionReference.action;
        _dashAction.Enable();

        Vector3 start = new Vector3(Random.Range(-3f, 3f), 1, Random.Range(-2f, 2f));
        GetComponent<NetworkTransform>().Teleport(start, Quaternion.identity, transform.localScale);
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            _dashAction?.Disable();
            _moveAction?.Disable();
        }
    }

    void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        if (GameManager.Instance.State.Value != GameState.Playing) return;

        _input = Vector2.ClampMagnitude(_moveAction.ReadValue<Vector2>(), 1f);
        Vector3 move = new Vector3(_input.x, 0f, _input.y);
        if (move.sqrMagnitude > 0.01f) _lastDirection = move.normalized;
        if (_dashAction.WasPressedThisFrame()) TryDash();

        bool isDashing = Time.time < _dashUntil;
        Vector3 velocity = isDashing ? _lastDirection * _dashSpeed : move * _speed;
        Vector3 p = transform.position + velocity * Time.deltaTime;
        p.x = Mathf.Clamp(p.x, -_arenaHalfSize.x, _arenaHalfSize.x);
        p.z = Mathf.Clamp(p.z, -_arenaHalfSize.y, _arenaHalfSize.y);
        transform.position = p;

        var target = Quaternion.LookRotation(_lastDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, _turnSpeed * Time.deltaTime);
    }

    private void TryDash()
    {
        if (!IsOwner || Time.time < _nextDashTime) return;
        _dashUntil = Time.time + _dashDuration;
        _nextDashTime = Time.time + _dashCooldown;
        _dashFx.Play();
        PlayDashFxRpc();
    }

    public float DashCoolDown => Mathf.Max(0f, _nextDashTime - Time.time);
    public float DashCoolDownRatio => DashCoolDown / _dashCooldown;

    [Rpc(SendTo.NotMe)]
    private void PlayDashFxRpc()
    {
        _dashFx.Play();
    }
}
