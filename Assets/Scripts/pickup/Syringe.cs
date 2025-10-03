using UnityEngine;

public class Syringe : PickUpObject
{
    [SerializeField]
    private float healAmount = 20f;

    [SerializeField]
    private PlayerHealth playerHealth;

    public override void Activate()
    {
        if (!WasHeldSinceLastCycle || !playerHealth)
            return;
        playerHealth.Heal(healAmount);
        Destroy(gameObject);
    }

    public void Inject()
    {
        if (!playerHealth)
            return;
        playerHealth.Heal(healAmount);
        Destroy(gameObject);
    }
}
