using UnityEngine;
using TMPro;

public class CheckpointNotificationUI : MonoBehaviour
{
    [SerializeField] private TMP_Text notificationText;

    private void OnEnable()
    {
        NetworkRaceManager.LocalCheckpointPassed += OnCheckpointPassed;
    }

    private void OnDisable()
    {
        NetworkRaceManager.LocalCheckpointPassed -= OnCheckpointPassed;
    }

    private void OnCheckpointPassed(int checkpointIndex, double elapsedTime)
    {
        if (notificationText == null)
            return;

        string checkpointName = checkpointIndex == 0
            ? "Финиш"
            : $"Чекпоинт {checkpointIndex}";

        notificationText.text =
            $"{checkpointName} пройден!\n" +
            $"Время от старта: {NetworkRaceManager.FormatTime(elapsedTime)}";
    }
}