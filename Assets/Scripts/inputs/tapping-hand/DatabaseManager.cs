using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// Firebase namespaces
using Firebase;
using Firebase.Database;
// Your shared models
using Telemetry;
using UnityEngine;

/// DatabaseManager
/// - Talks ONLY to Firebase Realtime Database
/// - Simple async writes (single + batch)
/// - Simple read of recent N samples
/// - Realtime subscribe to samples (ChildAdded)
/// - Added: Realtime subscribe to predictions (ChildAdded)
public class DatabaseManager : MonoBehaviour
{
    [Header("Realtime Database")]
    [Tooltip("Root path in your Firebase RTDB where telemetry is stored.")]
    [SerializeField]
    private string rootPath = "sessions";

    [Header("Startup")]
    [Tooltip(
        "If ON, this script runs Firebase dependency check by itself on Start(). Turn OFF if you already do it in a separate FirebaseInit."
    )]
    [SerializeField]
    private bool runDependencyCheck = true;

    // Cached DB root reference
    private DatabaseReference _dbRoot;
    public event Action OnDatabaseReady;

    // -------------------- Unity Lifecycle --------------------

    private async void Start()
    {
        if (runDependencyCheck)
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (status != DependencyStatus.Available)
            {
                Debug.LogError($"[DatabaseManager] Firebase dependencies not available: {status}");
                enabled = false;
                return;
            }
        }

        _dbRoot = FirebaseDatabase.DefaultInstance.RootReference;

        OnDatabaseReady?.Invoke();
    }

    // -------------------- PUBLIC WRITE METHODS --------------------

    /// Write a single DeviceSample under:
    ///   sessions/{sessionId}/samples/{autoPushKey}
    public async Task WriteSampleAsync(string sessionId, DeviceSample sample)
    {
        if (_dbRoot == null)
        {
            Debug.LogWarning("[DatabaseManager] WriteSampleAsync called before Firebase ready.");
            return;
        }

        string path = SamplesPath(sessionId);
        string key = _dbRoot.Push().Key; // unique child key
        string json = JsonUtility.ToJson(sample); // serialize to JSON

        await _dbRoot.Child(path).Child(key).SetRawJsonValueAsync(json);
    }

    /// Write a batch of samples under:
    ///   sessions/{sessionId}/batches/{batchTimestampMs}
    /// Note: We still recommend ALSO writing per-sample if you want easy queries/sorting.
    public async Task WriteBatchAsync(string sessionId, DeviceBatch batch)
    {
        if (_dbRoot == null)
        {
            Debug.LogWarning("[DatabaseManager] WriteBatchAsync called before Firebase ready.");
            return;
        }

        long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string path = $"{BatchesPath(sessionId)}/{ts}";
        string json = JsonUtility.ToJson(batch);

        await _dbRoot.Child(path).SetRawJsonValueAsync(json);
    }

    // -------------------- PUBLIC READ METHODS --------------------

    /// Read the most recent N samples (chronologically sorted).
    /// Uses an index on 'timestampMs' inside each sample.
    public async Task<List<DeviceSample>> ReadRecentSamplesAsync(string sessionId, int lastN = 100)
    {
        var results = new List<DeviceSample>();
        if (_dbRoot == null)
        {
            Debug.LogWarning(
                "[DatabaseManager] ReadRecentSamplesAsync called before Firebase ready."
            );
            return results;
        }

        var query = _dbRoot
            .Child(SamplesPath(sessionId))
            .OrderByChild("timestampMs")
            .LimitToLast(Mathf.Max(1, lastN));

        var snap = await query.GetValueAsync();
        if (snap == null || !snap.Exists)
            return results;

        foreach (var child in snap.Children)
        {
            var raw = child.GetRawJsonValue();
            if (!string.IsNullOrEmpty(raw))
                results.Add(JsonUtility.FromJson<DeviceSample>(raw));
        }

        // Ensure chronological (old → new)
        results.Sort((a, b) => a.timestampMs.CompareTo(b.timestampMs));
        return results;
    }

    // -------------------- REALTIME SUBSCRIBE --------------------

    /// Subscribe to new samples being added under sessions/{sessionId}/samples.
    /// Returns an IDisposable you should keep and Dispose() when you want to stop listening.
    ///
    /// NOTE: ChildAdded will fire once for EACH existing child (initial catch-up),
    /// then for new children as they are added.
    public IDisposable SubscribeToSamples(
        string sessionId,
        Action<DeviceSample> onSample,
        Action<string> onError = null
    )
    {
        if (_dbRoot == null)
        {
            Debug.LogWarning("[DatabaseManager] SubscribeToSamples called before Firebase ready.");
            // Return a no-op disposer so callers don't need null checks
            return new Disposer(() => { });
        }

        var query = _dbRoot.Child(SamplesPath(sessionId)).OrderByChild("timestampMs");

        // Handler for new children under the node
        EventHandler<ChildChangedEventArgs> handler = (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                onError?.Invoke(args.DatabaseError.Message);
                return;
            }

            var raw = args.Snapshot?.GetRawJsonValue();
            if (!string.IsNullOrEmpty(raw))
            {
                var sample = JsonUtility.FromJson<DeviceSample>(raw);
                onSample?.Invoke(sample);
            }
        };

        // Start listening
        query.ChildAdded += handler;

        // Return a disposer that will detach this listener
        return new Disposer(() => query.ChildAdded -= handler);
    }

    /// Subscribe to "sessions/predictions/latest" changes.
    /// Fires every time latest prediction is overwritten in Firebase.
    public IDisposable SubscribeToLatestPrediction(
        Action<Dictionary<string, int>> onPrediction,
        Action<string> onError = null
    )
    {
        if (_dbRoot == null)
        {
            Debug.LogWarning(
                "[DatabaseManager] SubscribeToLatestPrediction called before Firebase ready."
            );
            return new Disposer(() => { });
        }

        var query = _dbRoot.Child(PredictionsPath());

        // Handler for value changes on the "latest" node
        EventHandler<ValueChangedEventArgs> handler = (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                onError?.Invoke(args.DatabaseError.Message);
                return;
            }

            var snap = args.Snapshot;
            if (snap != null && snap.Exists)
            {
                var raw = snap.GetRawJsonValue();
                if (!string.IsNullOrEmpty(raw))
                {
                    try
                    {
                        var predictionData = JsonUtility
                            .FromJson<PredictionData>(raw)
                            .ToDictionary();
                        onPrediction?.Invoke(predictionData);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke($"Failed to parse latest prediction data: {ex.Message}");
                    }
                }
            }
        };

        // Start listening
        query.ValueChanged += handler;

        // Return disposer
        return new Disposer(() => query.ValueChanged -= handler);
    }

    // -------------------- PATH HELPERS --------------------

    public string SamplesPath(string sessionId) => $"{rootPath}/{sessionId}/samples";

    public string BatchesPath(string sessionId) => $"{rootPath}/{sessionId}/batches";

    public string PredictionsPath() => $"{rootPath}/predictions/latest";

    // -------------------- SMALL UTIL --------------------

    private sealed class Disposer : IDisposable
    {
        private readonly Action _dispose;
        private bool _done;

        public Disposer(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            if (_done)
                return;
            _done = true;
            _dispose?.Invoke();
        }
    }
}

[Serializable]
public class PredictionData
{
    public int tapping;
    public int hand;
    public int position;

    public Dictionary<string, int> ToDictionary()
    {
        return new Dictionary<string, int>
        {
            { "tapping", tapping },
            { "hand", hand },
            { "position", position },
        };
    }
}
