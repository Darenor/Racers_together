using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkCarController : NetworkBehaviour
{
    [Header("Arcade movement")]
    [SerializeField] private float acceleration = 16f;
    [SerializeField] private float maxSpeed = 22f;
    [SerializeField] private float reverseSpeed = 7f;
    [SerializeField] private float turnSpeed = 95f;
    [SerializeField] private float drag = 5f;

    private float speed;
    private float throttleInput;
    private float steeringInput;

    private void Update()
    {
        if (!IsOwner || !IsSpawned)
            return;

        throttleInput = Input.GetAxisRaw("Vertical");
        steeringInput = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate()
    {
        if (!IsSpawned)
            return;

        if (IsServer && IsOwner)
        {
            // Локальный ввод хоста применяется непосредственно на сервере.
            ApplyMovement(throttleInput, steeringInput);
        }
        else if (!IsServer && IsOwner)
        {
            // Удалённый клиент отправляет ввод хосту.
            SendInputServerRpc(throttleInput, steeringInput);
        }
    }

    [Rpc(SendTo.Server)]
    private void SendInputServerRpc(float throttle, float steering)
    {
        if (IsServer && !IsOwner)
            ApplyMovement(throttle, steering);
        else
            ApplyMovement(throttle, steering);
    }

    private void ApplyMovement(float throttle, float steering)
    {
        if (throttle > 0f)
            speed = Mathf.MoveTowards(speed, maxSpeed, acceleration * Time.fixedDeltaTime);
        else if (throttle < 0f)
            speed = Mathf.MoveTowards(speed, -reverseSpeed, acceleration * Time.fixedDeltaTime);
        else
            speed = Mathf.MoveTowards(speed, 0f, drag * Time.fixedDeltaTime);

        float direction = speed >= 0f ? 1f : -1f;
        float turn = steering * turnSpeed * direction * Time.fixedDeltaTime;
        transform.Rotate(0f, turn, 0f, Space.Self);
        transform.position += transform.forward * (speed * Time.fixedDeltaTime);
    }
}
