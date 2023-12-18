using Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponReloadedEvent))]
[RequireComponent(typeof(SetActiveWeaponEvent))]

[DisallowMultipleComponent]
public class ReloadWeapon : MonoBehaviour
{
    private ReloadWeaponEvent reloadWeaponEvent;
    private WeaponReloadedEvent weaponReloadedEvent;
    private SetActiveWeaponEvent setActiveWeaponEvent;
    private InventoryController inventoryController;
    private Coroutine reloadWeaponCoroutine;

    private void Awake()
    {
        // Load components
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponReloadedEvent = GetComponent<WeaponReloadedEvent>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        inventoryController = GetComponent<InventoryController>();
    }

    private void OnEnable()
    {
        // subscribe to reload weapon event
        reloadWeaponEvent.OnReloadWeapon += ReloadWeaponEvent_OnReloadWeapon;

        // Subscribe to set active weapon event
        setActiveWeaponEvent.OnSetActiveWeapon += SetActiveWeaponEvent_OnSetActiveWeapon;
    }

    private void OnDisable()
    {
        // unsubscribe from reload weapon event
        reloadWeaponEvent.OnReloadWeapon -= ReloadWeaponEvent_OnReloadWeapon;

        // Unsubscribe from set active weapon event
        setActiveWeaponEvent.OnSetActiveWeapon -= SetActiveWeaponEvent_OnSetActiveWeapon;
    }

    /// <summary>
    /// Handle reload weapon event
    /// </summary>
    private void ReloadWeaponEvent_OnReloadWeapon(ReloadWeaponEvent reloadWeaponEvent, ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        StartReloadWeapon(reloadWeaponEventArgs);
    }

    /// <summary>
    /// Start reloading the weapon
    /// </summary>
    private void StartReloadWeapon(ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        if (reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
        }

        reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(reloadWeaponEventArgs.weapon, reloadWeaponEventArgs.topUpAmmoPercent));
    }

    /// <summary>
    /// Reload weapon coroutine
    /// </summary>
    private IEnumerator ReloadWeaponRoutine(Weapon weapon, int topUpAmmoPercent)
    {
        // Retrieve the remaining ammo from the inventory controller
        int totalRemainingAmmo = 0;
        if (inventoryController != null)
        {
            totalRemainingAmmo = inventoryController.GetCurrentWeaponTotalAmmo();
        }

        // Check for no ammo and break
        if (totalRemainingAmmo == 0)
        {
            yield break;
        }

        // Play reload sound if there is one
        if (weapon.weaponDetails.weaponReloadingSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponReloadingSoundEffect);
        }

        // Set weapon as reloading
        weapon.isWeaponReloading = true;

        // Update reload progress timer
        while (weapon.weaponReloadTimer < weapon.weaponDetails.weaponReloadTime)
        {
            weapon.weaponReloadTimer += Time.deltaTime;
            yield return null;
        }

        // If total ammo is to be increased then update
        if (topUpAmmoPercent != 0)
        {
            int ammoIncrease = Mathf.RoundToInt((weapon.weaponDetails.weaponAmmoCapacity * topUpAmmoPercent) / 100f);

            int totalAmmo = weapon.weaponRemainingAmmo + ammoIncrease;

            if (totalAmmo > weapon.weaponDetails.weaponAmmoCapacity)
            {
                weapon.weaponRemainingAmmo = weapon.weaponDetails.weaponAmmoCapacity;
            }
            else
            {
                weapon.weaponRemainingAmmo = totalAmmo;
            }
        }

        // If the weapon has infinite ammo, simply refill the clip
        if (weapon.weaponDetails.hasInfiniteAmmo)
        {
            weapon.weaponClipRemainingAmmo = weapon.weaponDetails.weaponClipAmmoCapacity;
        }
        // If the ammo is not infinite, check if the remaining ammo is sufficient to
        // fully refill the clip; if so, refill the clip entirely
        else
        {
            int neededAmmo = weapon.weaponDetails.weaponClipAmmoCapacity - weapon.weaponClipRemainingAmmo;

            if (totalRemainingAmmo >= neededAmmo)
            {
                // Deduct the used ammo and set the clip to its maximum capacity
                totalRemainingAmmo -= weapon.weaponDetails.weaponClipAmmoCapacity - weapon.weaponClipRemainingAmmo;
                weapon.weaponClipRemainingAmmo = weapon.weaponDetails.weaponClipAmmoCapacity;
            }
            // Use all remaining ammo to partially refill the clip
            else
            {
                weapon.weaponClipRemainingAmmo += totalRemainingAmmo;
                totalRemainingAmmo = 0;
            }
        }

        // Set Weapon Remaining Ammo for none-infinite ammo weapon
        //if (!weapon.weaponDetails.hasInfiniteAmmo)
        //{
        //    //weapon.weaponRemainingAmmo -= weapon.weaponClipRemainingAmmo;
        //    totalRemainingAmmo -= weapon.weaponClipRemainingAmmo;
        //}

        // Reset weapon reload timer
        weapon.weaponReloadTimer = 0f;

        // Set weapon as not reloading
        weapon.isWeaponReloading = false;

        // Call weapon reloaded event
        weaponReloadedEvent.CallWeaponReloadedEvent(weapon, totalRemainingAmmo);

    }

    /// <summary>
    /// Set active weapon event handler
    /// </summary>
    private void SetActiveWeaponEvent_OnSetActiveWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        if (setActiveWeaponEventArgs.weapon.isWeaponReloading)
        {
            if (reloadWeaponCoroutine != null)
            {
                StopCoroutine(reloadWeaponCoroutine);
            }

            reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(setActiveWeaponEventArgs.weapon, 0));
        }
    }


}
