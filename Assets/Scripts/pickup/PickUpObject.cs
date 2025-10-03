using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PickUpObject : MonoBehaviour
{
    public bool isGrabbedBeforeRelease = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable =
            GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
    }

    public void Drop()
    {
        isGrabbedBeforeRelease = false;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbedBeforeRelease = true;
    }
}
