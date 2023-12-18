using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponReloadedEvent : MonoBehaviour
{
    public event Action<WeaponReloadedEvent, WeaponReloadedEventArgs> OnWeaponReloaded;

    public void CallWeaponReloadedEvent(Weapon weapon, int totalRemainingAmmo)
    {
        OnWeaponReloaded?.Invoke(this, new WeaponReloadedEventArgs() { weapon = weapon, totalRemainingAmmo = totalRemainingAmmo });
    }
}

public class WeaponReloadedEventArgs : EventArgs
{
    public Weapon weapon;
    public int totalRemainingAmmo;
}