using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _speed = 6f;
    [SerializeField] private float _turnSpeed = 720f;
    [SerializeField] private Vector2 _arenaHalfSize = new Vector2(7.5f, 4f);

    [SerializeField] private InputActionReference _moveActionReference;
    private InputAction _moveAction;
    private Vector2 _input;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        _moveAction = _moveActionReference.action;
        _moveAction.Enable();

        Vector3 start = new Vector3(Random.Range(-3f, 3f), 1, Random.Range(-2f, 2f));
        GetComponent<NetworkTransform>().Teleport(start, Quaternion.identity, transform.localScale);
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner) _moveAction?.Disable();
    }

    void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        _input = Vector2.ClampMagnitude(_moveAction.ReadValue<Vector2>(), 1f);
        Vector3 move = new Vector3(_input.x, 0f, _input.y);

        Vector3 p = transform.position + move * _speed * Time.deltaTime;
        p.x = Mathf.Clamp(p.x, -_arenaHalfSize.x, _arenaHalfSize.x);
        p.z = Mathf.Clamp(p.z, -_arenaHalfSize.y, _arenaHalfSize.y);
        transform.position = p;

        if(move.sqrMagnitude > 0.01f)
        {
            var target = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, _turnSpeed * Time.deltaTime);
        }
    }
}
