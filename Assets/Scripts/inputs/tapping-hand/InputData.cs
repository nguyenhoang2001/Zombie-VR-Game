using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Telemetry;
using static Configs;

/// Reads Meta Quest controller state at a fixed rate and raises an event with DeviceSample.
/// No Firebase code here—just input collection.
public class InputData : MonoBehaviour
{
    [Header("Sampling")]
    [SerializeField, Range(5, 120)] private int sampleRateHz = 100;   // how often to read controllers
    [SerializeField] private bool autoReacquire = true;               // re-find devices if invalid

    [Header("IDs (written into samples)")]
    [SerializeField] private string leftId  = Configs.LEFT_ID;
    [SerializeField] private string rightId = Configs.RIGHT_ID;

    // XR devices (kept public so you can watch them in Inspector)
    public InputDevice _leftController;
    public InputDevice _rightController;
    public InputDevice _HMD;

    // Public event: TelemetryManager subscribes to this
    public event Action<DeviceSample> OnSample;

    // Internal
    private float _interval;
    private float _accum;
    private readonly Dictionary<string, Vector3> _prevVel = new Dictionary<string, Vector3>();
    private readonly Dictionary<string, long> _prevTime = new Dictionary<string, long>();

    private void Awake()
    {
        _interval = 1f / Mathf.Max(1, sampleRateHz);
        InitializeInputDevices(); // try at startup
    }

    private void OnEnable()
    {
        // Also listen for devices appearing later (e.g., after Link connects)
        InputDevices.deviceConnected += OnDeviceConnected;
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    private void Update()
    {
        // Keep your device validation logic super simple
        if (autoReacquire && (!_rightController.isValid || !_leftController.isValid || !_HMD.isValid))
            InitializeInputDevices();

        // Fixed-rate sampling
        _accum += Time.unscaledDeltaTime;
        if (_accum < _interval) return;
        _accum = 0f;

        EmitControllerSample(_leftController,  leftId);
        EmitControllerSample(_rightController, rightId);
    }

    // ---- XR device (re)acquisition ----
    private void InitializeInputDevices()
    {
        if (!_rightController.isValid)
            GetFirst(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, ref _rightController);

        if (!_leftController.isValid)
            GetFirst(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, ref _leftController);

        if (!_HMD.isValid)
            GetFirst(InputDeviceCharacteristics.HeadMounted, ref _HMD);
    }

    private static void GetFirst(InputDeviceCharacteristics filter, ref InputDevice dev)
    {
        var list = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(filter, list);
        if (list.Count > 0) dev = list[0];
    }

    private void OnDeviceConnected(InputDevice dev)
    {
        // If a new device appears that matches what we need, grab it
        if ((dev.characteristics & InputDeviceCharacteristics.Left) != 0 &&
            (dev.characteristics & InputDeviceCharacteristics.Controller) != 0)
            _leftController = dev;

        if ((dev.characteristics & InputDeviceCharacteristics.Right) != 0 &&
            (dev.characteristics & InputDeviceCharacteristics.Controller) != 0)
            _rightController = dev;

        if ((dev.characteristics & InputDeviceCharacteristics.HeadMounted) != 0)
            _HMD = dev;
    }

    // ---- Read features & raise event ----
    private void EmitControllerSample(InputDevice dev, string deviceId)
    {
        if (!dev.isValid) return;

        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Pull available features (fallbacks are zeros/identity if not supported)
        Vector3 pos = TryGet(dev, CommonUsages.devicePosition, Vector3.zero);
        Vector3 vel = TryGet(dev, CommonUsages.deviceVelocity, Vector3.zero);

        // Calculate magnitude for velocity and acceleration
        float velMagnitude = vel.magnitude;

        // Simple acceleration: a = (v - vPrev) / dt
        Vector3 accelVec = Vector3.zero;
        float accelMagnitude = 0f;
        if (_prevVel.TryGetValue(deviceId, out var vPrev) && _prevTime.TryGetValue(deviceId, out var tPrev))
        {
            float dt = Mathf.Max(0.001f, (now - tPrev) / 1000f);
            accelVec = (vel - vPrev) / dt;
            accelMagnitude = accelVec.magnitude;
        }
        _prevVel[deviceId] = vel;
        _prevTime[deviceId] = now;

        var s = new DeviceSample
        {
            timestampMs     = now,
            deviceId        = deviceId,
            position        = pos,
            velocity        = velMagnitude,
            acceleration    = accelMagnitude
        };

        OnSample?.Invoke(s);
    }

    // Small helpers to reduce TryGetFeatureValue boilerplate
    private static Vector3 TryGet(InputDevice dev, InputFeatureUsage<Vector3> usage, Vector3 fallback)
    {
        if (dev.TryGetFeatureValue(usage, out var value))
            return value;
        return fallback;
    }

    private static Quaternion TryGet(InputDevice dev, InputFeatureUsage<Quaternion> usage, Quaternion fallback)
    {
        if (dev.TryGetFeatureValue(usage, out var value))
            return value;
        return fallback;
    }
}

