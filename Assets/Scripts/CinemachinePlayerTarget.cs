using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class CinemachinePlayerTarget : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();

        if (cam != null)
        {
            cam.Target.TrackingTarget = transform;
        }
        else
        {
            Debug.LogWarning("Cinemachine Camera не найдена в сцене!");
        }
    }
}
