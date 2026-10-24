using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class RaceHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text timerText;
    private float localStartTime;
    private bool localTimerStarted;

    private void Update()
    {
        var manager = NetworkRaceManager.Instance;
        if (manager == null)
        {
            SetStatus("Ожидание подключения...");
            return;
        }

        if (manager.RaceStarted.Value && !localTimerStarted)
        {
            localStartTime = Time.time + 3f;
            localTimerStarted = true;
        }

        if (!manager.RaceStarted.Value)
        {
            SetStatus("Ожидание старта");
            if (timerText != null) timerText.text = "00:00.000";
        }
        else if (manager.RaceFinished.Value)
        {
            SetStatus("Гонка завершена");
        }
        else if (localTimerStarted && Time.time >= localStartTime)
        {
            SetStatus("Гонка идёт");
            if (timerText != null)
                timerText.text = NetworkRaceManager.FormatTime(Time.time - localStartTime);
        }
        else
        {
            SetStatus("Приготовьтесь...");
        }
    }

    private void SetStatus(string value)
    {
        if (statusText != null) statusText.text = value;
    }
}
