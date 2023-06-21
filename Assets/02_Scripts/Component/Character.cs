using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtsyNetcode;

[RequireComponent(typeof(CharacterController))]
public class Character : NetEntity, IDamageable
{
    private bool ded;
    private float _health;

    public void TakeDamage(float damage)
    {
        _health -= damage;
        if (_health < 0)
        {
            Death();
        }
    }

    public void HealDamage(float heal)
    {
        _health += heal;
    }

    public void DropLoot()
    {
        throw new System.NotImplementedException();
    }

    public void Death()
    {
        Debug.Log("You died");
    }

    public bool isAlive()
    {
        return !ded;
    }

    public virtual void Teleportation(Vector3 positionTarget)
    {
        transform.position = positionTarget;
    }
}
