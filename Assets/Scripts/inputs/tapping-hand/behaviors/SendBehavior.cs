using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Telemetry;

public abstract class SendBehavior : MonoBehaviour
{
    protected DatabaseManager database;
    protected string sessionId;
    protected bool alsoWriteSingles;
    protected int activeHand = -1; // -1 none, 0 left, 1 right

    /// Call this from Sender once, after its refs are ready.
    public virtual void Setup(DatabaseManager db, string sessionId, bool alsoWriteSingles, int batchSize)
    {
        this.database = db;
        this.sessionId = sessionId;
        this.alsoWriteSingles = alsoWriteSingles;
    }

    /// Per-frame grip info. Exactly one of (leftHeld, rightHeld) may be true to allow buffering.
    public abstract void OnGripState(bool leftHeld, bool rightHeld);

    /// Called whenever InputData produces a sample.
    public abstract void OnSample(DeviceSample s);

    /// Optional per-frame tick (if needed by behavior).
    public virtual void Tick() { }

    protected virtual DeviceBatch BuildBatch(List<DeviceSample> samples)
    {
        return new DeviceBatch
        {
            sessionId = sessionId,
            samples = samples,
            tappingHand = (activeHand == 0 || activeHand == 1) ? activeHand : -1
        };
    }

    protected async Task WriteAsync(List<DeviceSample> toSend)
    {
        if (toSend == null || toSend.Count == 0) return;

        if (alsoWriteSingles)
        {
            foreach (var s in toSend)
                await database.WriteSampleAsync(sessionId, s);
        }

        var batch = BuildBatch(toSend);
        await database.WriteBatchAsync(sessionId, batch);
    }
}
