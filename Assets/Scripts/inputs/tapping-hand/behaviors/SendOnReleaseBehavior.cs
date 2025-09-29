using System.Collections.Generic;
using System.Threading.Tasks;
using Telemetry;
using UnityEngine;

public class SendOnReleaseBehavior : SendBehavior
{
    private readonly List<DeviceSample> recording = new List<DeviceSample>();
    private bool isRecording;
    private bool prevLeftHeld,
        prevRightHeld;
    private bool flushing;

    // --- Prediction wait state (no per-trial subscribe/unsubscribe) ---
    [SerializeField, Tooltip("How long to wait for a prediction after release (seconds).")]
    private float predictionTimeoutSec = 0.05f;

    private bool waitingForPrediction;
    private bool predictionReceived;
    private Coroutine predictionCo;

    private void OnEnable()
    {
        // Subscribe ONCE to all relevant prediction events
        EventManager.Subscribe(EventNames.TAPP_LEFT_WRIST, OnPredictionHit);
        EventManager.Subscribe(EventNames.TAPP_RIGHT_WRIST, OnPredictionHit);
        EventManager.Subscribe(EventNames.TAPP_LEFT_MID, OnPredictionHit);
        EventManager.Subscribe(EventNames.TAPP_RIGHT_MID, OnPredictionHit);
        EventManager.Subscribe(EventNames.TAPP_LEFT_ELBOW, OnPredictionHit);
        EventManager.Subscribe(EventNames.TAPP_RIGHT_ELBOW, OnPredictionHit);
        EventManager.Subscribe(EventNames.NO_TAPP, OnPredictionHit);
    }

    private void OnDisable()
    {
        // Unsubscribe ONCE
        EventManager.Unsubscribe(EventNames.TAPP_LEFT_WRIST, OnPredictionHit);
        EventManager.Unsubscribe(EventNames.TAPP_RIGHT_WRIST, OnPredictionHit);
        EventManager.Unsubscribe(EventNames.TAPP_LEFT_MID, OnPredictionHit);
        EventManager.Unsubscribe(EventNames.TAPP_RIGHT_MID, OnPredictionHit);
        EventManager.Unsubscribe(EventNames.TAPP_LEFT_ELBOW, OnPredictionHit);
        EventManager.Unsubscribe(EventNames.TAPP_RIGHT_ELBOW, OnPredictionHit);
        EventManager.Unsubscribe(EventNames.NO_TAPP, OnPredictionHit);
    }

    public override void OnGripState(bool leftHeld, bool rightHeld)
    {
        bool exactlyOne = leftHeld ^ rightHeld;

        // Start recording when exactly one grip is held
        if (exactlyOne && !isRecording)
        {
            isRecording = true;
            recording.Clear();
            activeHand = leftHeld ? 0 : 1;

            // Announce the beginning of a tap action window
            EventManager.Publish(EventNames.BEGIN_TAP);
        }

        // Stop & send when that grip session ends (transition from exactlyOne -> none or both)
        bool wasExactlyOne = prevLeftHeld ^ prevRightHeld;
        bool nowNoneOrBoth = (!leftHeld && !rightHeld) || (leftHeld && rightHeld);

        if (wasExactlyOne && nowNoneOrBoth)
        {
            // Send the whole recorded segment
            _ = FlushAsync();
            isRecording = false;
            activeHand = -1;

            StartPredictionWait();
        }

        prevLeftHeld = leftHeld;
        prevRightHeld = rightHeld;
    }

    public override void OnSample(DeviceSample s)
    {
        if (isRecording)
            recording.Add(s);
    }

    private async Task FlushAsync()
    {
        if (flushing || recording.Count == 0)
            return;
        flushing = true;

        var toSend = new List<DeviceSample>(recording);
        recording.Clear();

        Debug.Log($"[SendOnRelease] Flushing {toSend.Count} samples.");
        try
        {
            await WriteAsync(toSend);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[SendOnRelease] flush failed: {ex.Message}. Buffer kept.");
            recording.InsertRange(0, toSend);
        }
        finally
        {
            flushing = false;
        }
    }

    // ------------------ Prediction wait without per-trial (un)subscribe ------------------

    private void StartPredictionWait()
    {
        // Reset state
        waitingForPrediction = true;
        predictionReceived = false;

        // Cancel previous coroutine if any
        if (predictionCo != null)
            StopCoroutine(predictionCo);
        predictionCo = StartCoroutine(PredictionTimeout());
    }

    private System.Collections.IEnumerator PredictionTimeout()
    {
        float t = predictionTimeoutSec <= 0f ? 0.001f : predictionTimeoutSec;
        yield return new WaitForSeconds(t);

        if (waitingForPrediction && !predictionReceived)
        {
            EventManager.Publish(EventNames.NO_TAPP);
        }

        waitingForPrediction = false;
        predictionCo = null;
    }

    private void OnPredictionHit()
    {
        if (!waitingForPrediction)
            return;

        predictionReceived = true;
        waitingForPrediction = false;

        if (predictionCo != null)
        {
            StopCoroutine(predictionCo);
            predictionCo = null;
        }
    }
}
