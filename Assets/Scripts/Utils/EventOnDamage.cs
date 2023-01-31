using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventOnDamage : MonoBehaviour, Damageable
{
    public UnityEvent onDamage;

    public int health;
    public bool ignoreHealth;
    public bool disableOnZeroHealth;
    public UnityEvent onZeroHealth;

    public void Damage(int damageValue)
    {
        if (!isActiveAndEnabled)
            return;

        onDamage.Invoke();

        health -= damageValue;

        if (!ignoreHealth && health <= 0)
        {
            onZeroHealth.Invoke();

            if (disableOnZeroHealth)
                enabled = false;
        }
    }
}
