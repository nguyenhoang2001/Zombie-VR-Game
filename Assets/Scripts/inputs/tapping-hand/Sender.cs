using System;
using System.Collections.Generic;
using Telemetry;
using UnityEngine;
using UnityEngine.XR; // InputDevices / CommonUsages

public class Sender : MonoBehaviour
{
    [Header("References (assign in Inspector)")]
    [SerializeField]
    private InputData inputData; // producer

    [SerializeField]
    private DatabaseManager database; // Firebase access

    [SerializeField]
    private SendBehavior behavior; // <- Pick one: SendByBatchBehavior OR SendOnReleaseBehavior

    [Header("Session")]
    [Tooltip("If empty, a random session id is generated at runtime.")]
    [SerializeField]
    private string sessionId = "";

    [Header("Upload behavior (used by some strategies)")]
    [SerializeField, Range(1, 200)]
    private int batchSize = 100; // used by SendByBatchBehavior

    [SerializeField]
    private bool alsoWriteSingles = false;

    // Internal
    private InputDevice _leftDev,
        _rightDev;

    private bool _prevLeftHeld,
        _prevRightHeld;

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            sessionId = Guid.NewGuid().ToString("N");
        EnsureDevices();
    }

    private void OnEnable()
    {
        if (!behavior)
            behavior = GetComponent<SendBehavior>();

        if (behavior == null)
        {
            Debug.LogError(
                "Sender: No SendBehavior assigned. Add SendByBatchBehavior or SendOnReleaseBehavior."
            );
            enabled = false;
            return;
        }

        behavior.Setup(database, sessionId, alsoWriteSingles, batchSize);

        if (inputData != null)
            inputData.OnSample += OnSample;
        InputDevices.deviceConnected += OnDeviceConnected;
    }

    private void OnDisable()
    {
        if (inputData != null)
            inputData.OnSample -= OnSample;
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    private void OnDeviceConnected(InputDevice dev) => EnsureDevices();

    private void EnsureDevices()
    {
        if (!_leftDev.isValid)
            _leftDev = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (!_rightDev.isValid)
            _rightDev = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        EnsureDevices();

        // Read current grip state using button if possible, otherwise analog grip > 0.5
        var (leftHeld, rightHeld) = GetGripHeld();
        behavior.OnGripState(leftHeld, rightHeld);
        behavior.Tick();

        _prevLeftHeld = leftHeld;
        _prevRightHeld = rightHeld;
    }

    private (bool leftHeld, bool rightHeld) GetGripHeld()
    {
        bool left = false,
            right = false;

        if (_leftDev.isValid)
        {
            if (!_leftDev.TryGetFeatureValue(CommonUsages.gripButton, out left))
            {
                if (_leftDev.TryGetFeatureValue(CommonUsages.grip, out float gv))
                    left = gv > 0.5f;
            }
        }

        if (_rightDev.isValid)
        {
            if (!_rightDev.TryGetFeatureValue(CommonUsages.gripButton, out right))
            {
                if (_rightDev.TryGetFeatureValue(CommonUsages.grip, out float gv))
                    right = gv > 0.5f;
            }
        }

        // Treat both held as "no update anything"
        if (left && right)
            return (false, false);
        return (left, right);
    }

    private void OnSample(DeviceSample s)
    {
        behavior.OnSample(s);
    }

    // ---------------- Optional helpers unchanged ----------------
    public System.Threading.Tasks.Task<List<DeviceSample>> LoadRecentAsync(int lastN = 100) =>
        database.ReadRecentSamplesAsync(sessionId, lastN);

    public IDisposable StartLiveSubscribe(
        Action<DeviceSample> onSample,
        Action<string> onError = null
    ) => database.SubscribeToSamples(sessionId, onSample, onError);
}
