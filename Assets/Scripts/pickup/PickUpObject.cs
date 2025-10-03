using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class PickUpObject : MonoBehaviour
{
    [Tooltip("If true, this pickup will block weapon swap for the current cycle when it was held.")]
    [SerializeField]
    private bool blocksSwap = true;
    public bool BlocksSwap => blocksSwap;

    /// True if this object has been held at least once since last manager ResetCycle().
    public bool WasHeldSinceLastCycle { get; private set; }

    /// Time.time when it was last grabbed (used to choose the 'most recent' object).
    public float LastHeldTime { get; private set; }

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    protected virtual void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.selectEntered.AddListener(OnSelectEntered);
        grab.selectExited.AddListener(OnSelectExited);
    }

    protected virtual void OnEnable()
    {
        (PickUpManager.Instance ?? FindObjectOfType<PickUpManager>())?.Register(this);
    }

    protected virtual void OnDisable()
    {
        PickUpManager.Instance?.Unregister(this);
    }

    protected virtual void OnDestroy()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnSelectEntered);
            grab.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs _)
    {
        WasHeldSinceLastCycle = true;
        LastHeldTime = Time.time;
    }

    private void OnSelectExited(
        SelectExitEventArgs _
    ) { /* keep flag sticky */
    }

    public void ResetCycleFlag() => WasHeldSinceLastCycle = false;

    /// Called by PickUpManager when this pickup should “do its action” for a tap.
    /// Default = no-op; override in derived pickups (e.g., Syringe.Inject).
    public virtual void Activate() { }
}
