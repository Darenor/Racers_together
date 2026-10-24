using UnityEngine;
using Unity.Netcode;

public class HostOnlyButton : MonoBehaviour
{
    [SerializeField] private GameObject startRaceButton;

    private void Start()
    {
        if (startRaceButton == null)
        {
            Debug.LogError("HostOnlyButton: не назначена кнопка!");
            return;
        }

        UpdateButton();
    }

    private void Update()
    {
        UpdateButton();
    }

    private void UpdateButton()
    {
        if (startRaceButton == null)
            return;

        bool isHost = NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsHost;

        startRaceButton.SetActive(isHost);
    }
}
