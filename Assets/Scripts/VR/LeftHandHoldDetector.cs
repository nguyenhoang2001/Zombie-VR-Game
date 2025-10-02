using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor))]
public class LeftHandHoldDetector : MonoBehaviour
{
    public bool IsHolding { get; private set; } = false;
    public bool IsHoldingObject = false;

    UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;

    private void Awake()
    {
        interactor =
            GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();
        interactor.selectEntered.AddListener(OnSelectEnter);
        interactor.selectExited.AddListener(OnSelectExit);
    }

    private void OnDestroy()
    {
        if (interactor != null)
        {
            interactor.selectEntered.RemoveListener(OnSelectEnter);
            interactor.selectExited.RemoveListener(OnSelectExit);
        }
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        IsHolding = true;
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        IsHolding = false;
    }
}
