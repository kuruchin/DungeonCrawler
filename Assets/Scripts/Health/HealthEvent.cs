using System;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthEvent : MonoBehaviour
{
    public event Action<HealthEvent, HealthEventArgs> OnHealthChanged;

    public void CallHealthChangedEvent(float healthPercent, int healthAmount, int damageAmount, AmmoType ammoType)
    {
        OnHealthChanged?.Invoke(this, new HealthEventArgs() { healthPercent = healthPercent, healthAmount = healthAmount, damageAmount = damageAmount , ammoType = ammoType });

    }
}

public class HealthEventArgs : EventArgs
{
    public float healthPercent;
    public int healthAmount;
    public int damageAmount;
    public AmmoType ammoType;
}