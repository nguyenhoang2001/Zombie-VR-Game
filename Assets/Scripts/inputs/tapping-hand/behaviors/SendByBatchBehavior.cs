using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Telemetry;

public class SendByBatchBehavior : SendBehavior
{
    [SerializeField, Range(1, 200)] private int batchSize = 100;

    private readonly List<DeviceSample> buffer = new List<DeviceSample>();
    private bool allowBuffering;
    private bool prevLeftHeld, prevRightHeld;
    private bool flushing;

    public override void Setup(DatabaseManager db, string sessionId, bool alsoWriteSingles, int batchSize)
    {
        base.Setup(db, sessionId, alsoWriteSingles, batchSize);
        if (batchSize > 0) this.batchSize = batchSize;
    }

    public override void OnGripState(bool leftHeld, bool rightHeld)
    {
        bool exactlyOne = leftHeld ^ rightHeld;
        allowBuffering = exactlyOne;
        if (exactlyOne)
            activeHand = leftHeld ? 0 : 1;

        bool wasExactlyOne = prevLeftHeld ^ prevRightHeld;
        bool nowNoneOrBoth = (!leftHeld && !rightHeld) || (leftHeld && rightHeld);
        if (wasExactlyOne && nowNoneOrBoth)
        {
            // on release (or both), try flush if batch full, then clear
            if (buffer.Count >= batchSize)
                _ = FlushAsync();
            buffer.Clear();
            activeHand = -1;
        }

        prevLeftHeld = leftHeld;
        prevRightHeld = rightHeld;

        // Always check ready
        if (buffer.Count >= batchSize)
            _ = FlushAsync();
    }

    public override void OnSample(DeviceSample s)
    {
        if (allowBuffering)
            buffer.Add(s);
    }

    private async Task FlushAsync()
    {
        if (flushing || buffer.Count == 0) return;
        flushing = true;

        var toSend = new List<DeviceSample>(buffer);
        buffer.Clear();

        try
        {
            await WriteAsync(toSend);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[SendByBatch] flush failed: {ex.Message}. Buffer kept.");
            buffer.InsertRange(0, toSend);
        }
        finally
        {
            flushing = false;
        }
    }
}
