using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RaceCheckpoint : MonoBehaviour
{
    public NetworkRaceManager raceManager;
    [Min(0)] public int checkpointIndex;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var networkObject = other.GetComponentInParent<NetworkObject>();
        if (networkObject == null || !networkObject.IsSpawned) return;
        if (raceManager == null) raceManager = NetworkRaceManager.Instance;
        if (raceManager == null || !NetworkManager.Singleton.IsServer) return;

        raceManager.RegisterCheckpoint(networkObject.OwnerClientId, checkpointIndex);
    }
}
