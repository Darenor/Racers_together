using Unity.Netcode;
using UnityEngine;

public class NetworkRaceManager : NetworkBehaviour
{
    public static NetworkRaceManager Instance { get; private set; }

    [SerializeField, Min(1)] private int lapsToWin = 3;
    [SerializeField, Min(1)] private int lastCheckpointIndex = 3;

    public NetworkVariable<bool> RaceStarted = new(false);
    public NetworkVariable<bool> RaceFinished = new(false);

    private readonly NetworkVariable<double> startServerTime = new(0);
    private readonly System.Collections.Generic.Dictionary<ulong, PlayerRaceData> players =
        new System.Collections.Generic.Dictionary<ulong, PlayerRaceData>();
    public static event System.Action<int, double> LocalCheckpointPassed;

    private class PlayerRaceData
    {
        public int laps;
        public int nextCheckpoint = 1;
        public double lapStart;
        public double totalTime;
        public double bestLap = double.PositiveInfinity;
        public bool finished;
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            foreach (ulong id in NetworkManager.ConnectedClientsIds)
                EnsurePlayer(id);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager != null && IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        if (Instance == this) Instance = null;
    }

    private void OnClientConnected(ulong id) => EnsurePlayer(id);
    private void OnClientDisconnected(ulong id) => players.Remove(id);

    private void EnsurePlayer(ulong id)
    {
        if (!players.ContainsKey(id))
            players[id] = new PlayerRaceData();
    }

    [Rpc(SendTo.Server)]
    public void RequestStartRaceRpc()
    {
        if (!IsServer || RaceStarted.Value) return;
        startServerTime.Value = NetworkManager.ServerTime.Time + 3.0;
        RaceStarted.Value = true;
        foreach (ulong id in NetworkManager.ConnectedClientsIds)
        {
            EnsurePlayer(id);
            players[id].laps = 0;
            players[id].nextCheckpoint = 1;
            players[id].lapStart = startServerTime.Value;
            players[id].totalTime = 0;
            players[id].bestLap = double.PositiveInfinity;
            players[id].finished = false;
        }
    }

    public bool IsRaceLive => RaceStarted.Value && !RaceFinished.Value &&
                              NetworkManager != null &&
                              NetworkManager.ServerTime.Time >= startServerTime.Value;

    
    public void RegisterCheckpoint(ulong playerId, int checkpointIndex)
    {
        if (!IsServer || !IsRaceLive || !players.ContainsKey(playerId))
            return;

        PlayerRaceData data = players[playerId];

        if (data.finished || checkpointIndex != data.nextCheckpoint)
            return;

        double now = NetworkManager.ServerTime.Time;
        double elapsed = now - startServerTime.Value;

        NotifyCheckpointRpc(playerId, checkpointIndex, elapsed);

        if (checkpointIndex == 0)
        {
            double lapTime = now - data.lapStart;

            if (lapTime > 0 && lapTime < data.bestLap)
                data.bestLap = lapTime;

            data.laps++;
            data.totalTime += lapTime;
            data.lapStart = now;

            if (data.laps >= lapsToWin)
            {
                data.finished = true;

                bool allFinished = true;

                foreach (var entry in players.Values)
                {
                    if (!entry.finished)
                        allFinished = false;
                }

                if (allFinished)
                    RaceFinished.Value = true;
            }
            else
            {
                data.nextCheckpoint = 1;
            }
        }
        else
        {
            if (checkpointIndex >= lastCheckpointIndex)
                data.nextCheckpoint = 0;
            else
                data.nextCheckpoint++;
        }
    }


    public string GetResultsText()
    {
        if (!IsServer) return "Результаты рассчитывает хост.";
        var text = "РЕЗУЛЬТАТЫ\n";
        foreach (var entry in players)
        {
            string best = double.IsPositiveInfinity(entry.Value.bestLap)
                ? "--:--.---"
                : FormatTime(entry.Value.bestLap);
            text += $"Игрок {entry.Key + 1}: круги {entry.Value.laps}/{lapsToWin}, " +
                    $"лучший круг {best}, всего {FormatTime(entry.Value.totalTime)}\n";
        }
        return text;
    }

    public static string FormatTime(double seconds)
    {
        if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0)
            return "--:--.---";
        int minutes = (int)(seconds / 60);
        double remainder = seconds - minutes * 60;
        return $"{minutes:00}:{remainder:00.000}";
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyCheckpointRpc(
        ulong playerId,
        int checkpointIndex,
        double elapsedTime)
    {
        if (NetworkManager.Singleton == null)
            return;

        if (NetworkManager.Singleton.LocalClientId != playerId)
            return;

        LocalCheckpointPassed?.Invoke(checkpointIndex, elapsedTime);
    }
}
