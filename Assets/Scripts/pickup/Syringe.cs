using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Syringe : MonoBehaviour
{
    [SerializeField]
    private float healAmount = 20f;

    [SerializeField]
    private PlayerHealth playerHealth;

    private bool isGrabbedBeforeRelease = false;
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

    public void Inject()
    {
        Debug.Log("Inject called");
        if (!isGrabbedBeforeRelease)
            return;

        Debug.Log("Inject proceeding");

        if (playerHealth.GetHealth() >= 100f)
        {
            isGrabbedBeforeRelease = false;
            return;
        }

        Debug.Log("Injected");

        playerHealth?.Heal(healAmount);
        Destroy(gameObject);
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
