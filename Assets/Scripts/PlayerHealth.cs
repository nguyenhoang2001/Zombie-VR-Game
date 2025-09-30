using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    float hitPoints = 100f;
    const float maxHitPoints = 100f;

    [SerializeField]
    private HealthBar healthBar;

    private bool isDead = false;

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        hitPoints -= damage;
        hitPoints = Mathf.Clamp(hitPoints, 0, maxHitPoints);

        healthBar.UpdateHealth(maxHitPoints, hitPoints);

        if (hitPoints <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(WaitForHealthBarThenDie());
        }
    }

    private IEnumerator WaitForHealthBarThenDie()
    {
        // Wait until the health bar reaches the target
        while (!healthBar.IsAtTarget())
        {
            yield return null;
        }

        GetComponent<DeathHandler>().HandleDeath();
    }
}
