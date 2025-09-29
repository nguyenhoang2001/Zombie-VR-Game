using System;
using System.Collections.Generic;
using UnityEngine;

/// Small, shared data types for samples/batches.
/// Keep this file tiny so other scripts can "using" it easily.
namespace Telemetry
{
    [Serializable]
    public class DeviceSample
    {
        public long timestampMs;            // UTC time in ms
        public string deviceId;             // "LeftController" / "RightController"
        public Vector3 position;            // world position (XR origin space)
        public float velocity;            // m/s (if available; else zero)
        public float acceleration;        // m/s^2 (computed from Δv/Δt)
    }

    [Serializable]
    public class DeviceBatch
    {
        public string sessionId;
        public List<DeviceSample> samples = new List<DeviceSample>();
        public int tappingHand; // 0 = left, 1 = right
    }
}
