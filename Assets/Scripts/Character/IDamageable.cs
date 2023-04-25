using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage);
    void HealDamage(float heal);
    void DropLoot();
    void Death();
    bool isAlive();
}
