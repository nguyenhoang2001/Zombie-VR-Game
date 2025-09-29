using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Telemetry;

public class Receiver : MonoBehaviour
{
    [SerializeField] private DatabaseManager databaseManager;
    [SerializeField] private Sender sender;

    private IDisposable _predictionSubscription;
    private bool isFirstPrediction = true;

    private void Start()
    {
        databaseManager.OnDatabaseReady += SubscribeToPredictions;
    }

    private void SubscribeToPredictions()
    {
        _predictionSubscription = databaseManager.SubscribeToLatestPrediction(OnNewPrediction, OnPredictionError);
    }

    private void OnDestroy()
    {
        _predictionSubscription?.Dispose();
    }

    private void OnNewPrediction(Dictionary<string, int> predictionData)
    {
        if (isFirstPrediction)
        {
            isFirstPrediction = false;
            return;
        }

        if (predictionData.TryGetValue("tapping", out int tapping) && tapping == 1)
        {
            if (predictionData.TryGetValue("hand", out int hand) &&
                predictionData.TryGetValue("position", out int position))
            {
                TriggerPredictionEvent(hand, position);
            }
        }
        else if(tapping == 0)
        {
            EventManager.Publish(EventNames.NO_TAPP);
        }
    }

    private void OnPredictionError(string error)
    {
        Debug.LogError($"Prediction subscription error: {error}");
    }

    private void TriggerPredictionEvent(int hand, int position)
    {
        string eventName = null;

        if (hand == 0)
        {
            switch (position)
            {
                case 0:
                    eventName = EventNames.TAPP_LEFT_WRIST;
                    break;
                case 1:
                    eventName = EventNames.TAPP_LEFT_MID;
                    break;
                case 2:
                    eventName = EventNames.TAPP_LEFT_ELBOW;
                    break;
            }
        }
        else if (hand == 1)
        {
            switch (position)
            {
                case 0:
                    eventName = EventNames.TAPP_RIGHT_WRIST;
                    break;
                case 1:
                    eventName = EventNames.TAPP_RIGHT_MID;
                    break;
                case 2:
                    eventName = EventNames.TAPP_RIGHT_ELBOW;
                    break;
            }
        }

        if (eventName != null)
        {
            EventManager.Publish(eventName);
        }
    }
}