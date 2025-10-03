using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class GunReticleFollower : MonoBehaviour
{
    [Header("Aim Ray (use same as VRWeapon)")]
    public Transform rayOrigin; // Right controller / Muzzle
    public float maxDistance = 100f;
    public LayerMask hitMask = ~0; // Match VRWeapon's hit mask
    public float surfaceOffset = 0.005f; // Push off surface to avoid z-fighting
    public bool alignToSurface = true; // Or face the camera instead

    [Header("View (optional)")]
    public Camera playerCamera; // Main/VR camera
    public bool hideWhenNoHit = false;
    public float followSmooth = 0f; // 0 = snap, >0 = smooth follow

    Transform tr;
    Vector3 vel;

    void Awake()
    {
        tr = transform;
        if (!playerCamera)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (!rayOrigin)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        if (Physics.Raycast(ray, out var hit, maxDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            Vector3 targetPos = hit.point + hit.normal * surfaceOffset;
            if (followSmooth > 0f)
                tr.position = Vector3.SmoothDamp(tr.position, targetPos, ref vel, followSmooth);
            else
                tr.position = targetPos;

            if (alignToSurface)
                tr.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up); // “decal” style
            else if (playerCamera)
                tr.rotation = Quaternion.LookRotation(
                    tr.position - playerCamera.transform.position
                );
        }
        else
        {
            if (hideWhenNoHit)
            {
                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
                return;
            }

            // fallback: place at end of ray
            Vector3 targetPos = ray.origin + ray.direction * maxDistance;
            tr.position = targetPos;
            if (playerCamera)
                tr.rotation = Quaternion.LookRotation(
                    tr.position - playerCamera.transform.position
                );
        }
    }
}
