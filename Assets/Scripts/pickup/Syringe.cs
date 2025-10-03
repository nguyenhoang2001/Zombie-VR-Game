using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Syringe : PickUpObject
{
    [SerializeField]
    private float healAmount = 20f;

    [SerializeField]
    private PlayerHealth playerHealth;

    public void Inject()
    {
        if (playerHealth.GetHealth() >= 100f)
        {
            isGrabbedBeforeRelease = false;
            return;
        }

        Debug.Log("Injected");

        playerHealth?.Heal(healAmount);
        Destroy(gameObject);
    }
}
