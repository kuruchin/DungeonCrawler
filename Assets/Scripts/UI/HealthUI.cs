using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthUI : MonoBehaviour
{
    private TextMeshProUGUI healthPoints;

    private void Awake()
    {
        healthPoints = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        SetHealthBar(healthEventArgs);

    }

    private void SetHealthBar(HealthEventArgs healthEventArgs)
    {
        healthPoints.text = healthEventArgs.healthAmount.ToString();
    }
}