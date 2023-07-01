using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScareCrow : Enemy
{
    public override void TakeDamage(float damage)
    {
        damageSameTime += damage;
    }
}