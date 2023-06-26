using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScareCrow : Enemy
{
    
    private bool isHealing = false;
    
    public new void TakeDamage(float damage)
    {
        // TODO radiant couleur en fonction degats
        UIManager.instance.CreateFloatingText(damage.ToString(), transform.position + Vector3.up, Color.red);
        if (_health - damage < 1)
        {
            return;
        }
        _health -= damage;
    }

    private IEnumerator Healing()
    {
        if (_health > _maxHealth)
        {
            yield break;
        }
        yield return new WaitForSeconds(1);
        HealDamage(_maxHealth);
    }

    private void Update()
    {
        if (_health < _maxHealth && !isHealing)
        {
            isHealing = true;
            StartCoroutine(Healing());
        }
    }
}
