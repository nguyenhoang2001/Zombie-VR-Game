using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRWeaponSwitcher : MonoBehaviour
{
    [SerializeField]
    int currentWeapon = 0;

    // Drag your left-hand interactor's LeftHandHoldDetector here in the Inspector
    [SerializeField]
    LeftHandHoldDetector leftHandDetector;

    [SerializeField]
    private PickUpManager pickUpManager;

    void Start()
    {
        SetWeaponActive();
    }

    void Update()
    {
        int previousWeapon = currentWeapon;

        if (previousWeapon != currentWeapon)
        {
            SetWeaponActive();
        }
    }

    public void NextWeapon()
    {
        if (pickUpManager != null && pickUpManager.isHoldingAnObject)
            return;

        DoNextWeapon();
    }

    public void DoNextWeapon()
    {
        int previousWeapon = currentWeapon;

        if (currentWeapon >= transform.childCount - 1)
        {
            currentWeapon = 0;
        }
        else
        {
            currentWeapon++;
        }

        if (previousWeapon != currentWeapon)
        {
            SetWeaponActive();
        }
    }

    private void SetWeaponActive()
    {
        int weaponIndex = 0;

        foreach (Transform weapon in transform)
        {
            if (weaponIndex == currentWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            weaponIndex++;
        }
    }
}
