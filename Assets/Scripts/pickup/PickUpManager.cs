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
    private List<PickUpObject> trackedObjects = new List<PickUpObject>();

    public bool IsLeftHandGrippingAnyTracked()
    {
        if (leftHandInteractor == null)
            return false;

        foreach (var obj in trackedObjects)
        {
            if (obj != null && obj.isGrabbedBeforeRelease)
            {
                return true;
            }
        }
        return false;
    }
}
