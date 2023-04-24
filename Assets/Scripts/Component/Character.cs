using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtsyNetcode;

[RequireComponent(typeof(CharacterController))]
public class Character : NetEntity, IDamageable
{
    private float _health;

    public void TakeDamage()
    {
        throw new System.NotImplementedException();
    }

    public void HealDamage()
    {
        throw new System.NotImplementedException();
    }

    public void DropLoot()
    {
        throw new System.NotImplementedException();
    }

    public void Death()
    {
        throw new System.NotImplementedException();
    }

    public bool isAlive()
    {
        throw new System.NotImplementedException();
    }
}
