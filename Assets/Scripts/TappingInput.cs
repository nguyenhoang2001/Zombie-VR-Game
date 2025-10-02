using UnityEngine;
using UnityEngine.Events;

public class TappingInput : MonoBehaviour
{
    [Header("Left Arm Events")]
    public UnityEvent OnLeftWristTap;
    public UnityEvent OnLeftElbowTap;

    [Header("Right Arm Events")]
    public UnityEvent OnRightWristTap;
    public UnityEvent OnRightElbowTap;

    [Header("No Tap Events")]
    public UnityEvent OnNoTapEvent;

    public void TriggerLeftWristTap()
    {
        OnLeftWristTap?.Invoke();
    }

    public void TriggerLeftElbowTap()
    {
        OnLeftElbowTap?.Invoke();
    }

    public void TriggerRightWristTap()
    {
        OnRightWristTap?.Invoke();
    }

    public void TriggerRightElbowTap()
    {
        OnRightElbowTap?.Invoke();
    }

    public void TriggerNoTap()
    {
        OnNoTapEvent?.Invoke();
    }

    private void OnEnable()
    {
        EventManager.Subscribe(EventNames.TAPP_LEFT_WRIST, TriggerLeftWristTap);
        EventManager.Subscribe(EventNames.TAPP_LEFT_ELBOW, TriggerLeftElbowTap);
        EventManager.Subscribe(EventNames.TAPP_RIGHT_WRIST, TriggerRightWristTap);
        EventManager.Subscribe(EventNames.TAPP_RIGHT_ELBOW, TriggerRightElbowTap);
        EventManager.Subscribe(EventNames.NO_TAPP, TriggerNoTap);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe(EventNames.TAPP_LEFT_WRIST, TriggerLeftWristTap);
        EventManager.Unsubscribe(EventNames.TAPP_LEFT_ELBOW, TriggerLeftElbowTap);
        EventManager.Unsubscribe(EventNames.TAPP_RIGHT_WRIST, TriggerRightWristTap);
        EventManager.Unsubscribe(EventNames.TAPP_RIGHT_ELBOW, TriggerRightElbowTap);
        EventManager.Unsubscribe(EventNames.NO_TAPP, TriggerNoTap);
    }
}
