using UnityEngine;

public class VRWeaponSwitcher : MonoBehaviour
{
    [SerializeField]
    private int currentWeapon = 0;

    public void NextWeapon()
    {
        if (PickUpManager.Instance != null && PickUpManager.Instance.AnyHeldThisCycle)
        {
            Debug.Log("[WeaponSwitcher] Swap BLOCKED: a pickup was held this cycle.");
            return;
        }

        currentWeapon = (currentWeapon + 1) % transform.childCount;

        int i = 0;
        foreach (Transform t in transform)
            t.gameObject.SetActive(i++ == currentWeapon);
    }

    // Call this after your ML/tap event completes to start a fresh cycle.
    public void ResetCycleAfterDecision()
    {
        PickUpManager.Instance?.ResetCycle();
    }
}
