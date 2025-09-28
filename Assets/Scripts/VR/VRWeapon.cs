using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using TMPro;

public class VRWeapon : MonoBehaviour
{
    [Header("Shoot Origin")]
    [SerializeField] Transform rayOrigin; // <- set this to Right Controller or a Muzzle child

    [Header("References")]
    [SerializeField] Camera FPCamera;                // optional fallback
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] GameObject hitEffect;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] Ammo ammoSlot;
    [SerializeField] AmmoType ammoType;

    [Header("Stats")]
    [SerializeField] float range = 100f;
    [SerializeField] float damage = 30f;
    [SerializeField] float timeBetweenShots = 0.2f;
    [SerializeField] LayerMask hitLayers = ~0;       // hit everything by default

    [Header("Input")]
    [SerializeField] XRInputValueReader<float> m_TriggerInput = new XRInputValueReader<float>("Trigger");

    bool canShoot = true;

    void Awake()
    {
        // Fallback to camera if Ray Origin not assigned (useful in editor)
        if (rayOrigin == null && FPCamera != null) rayOrigin = FPCamera.transform;
    }

    void OnEnable() => canShoot = true;

    void Update()
    {
        if (ammoText) ammoText.text = ammoSlot.GetCurrentAmmo(ammoType).ToString();

        // fire when right trigger is pressed past a small threshold
        if (canShoot && m_TriggerInput.ReadValue() >= 0.1f)
            StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        canShoot = false;

        if (ammoSlot.GetCurrentAmmo(ammoType) > 0)
        {
            if (muzzleFlash) muzzleFlash.Play();
            ProcessRaycast();
            ammoSlot.ReduceCurrentAmmo(ammoType);
        }

        yield return new WaitForSeconds(timeBetweenShots);
        canShoot = true;
    }

    void ProcessRaycast()
    {
        if (!rayOrigin)
        {
            Debug.LogWarning("VRWeapon: Ray Origin not set.");
            return;
        }

        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * range, Color.red, 0.05f);

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward,
                            out RaycastHit hit, range, hitLayers, QueryTriggerInteraction.Ignore))
        {
            if (hitEffect)
            {
                var impact = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 0.1f);
            }

            var target = hit.transform.GetComponent<EnemyHealth>();
            if (target) target.TakeDamage(damage);
        }
    }
}
