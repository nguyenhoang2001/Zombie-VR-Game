using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PickUpManager : MonoBehaviour
{
    [Header("Left Hand Interactor")]
    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor leftHandInteractor;

    [Header("Trackable Objects")]
    [SerializeField]
    private List<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable> trackedObjects =
        new List<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

    public bool isHoldingAnObject { get; private set; } = false;

    private void OnEnable()
    {
        foreach (var obj in trackedObjects)
        {
            if (obj != null)
            {
                obj.selectEntered.AddListener(OnGrabbed);
                obj.selectExited.AddListener(OnReleased);
            }
        }
    }

    private void OnDisable()
    {
        foreach (var obj in trackedObjects)
        {
            if (obj != null)
            {
                obj.selectEntered.RemoveListener(OnGrabbed);
                obj.selectExited.RemoveListener(OnReleased);
            }
        }
    }

    private void Update()
    {
        isHoldingAnObject = IsLeftHandGrippingAnyTracked();
    }

    private void OnGrabbed(SelectEnterEventArgs args) => Update();

    private void OnReleased(SelectExitEventArgs args) => Update();

    private bool IsLeftHandGrippingAnyTracked()
    {
        if (leftHandInteractor == null)
            return false;

        foreach (var obj in trackedObjects)
        {
            if (obj != null && leftHandInteractor.interactablesSelected.Contains(obj))
            {
                return true;
            }
        }
        return false;
    }
}
