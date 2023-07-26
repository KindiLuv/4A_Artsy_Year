using System.Numerics;
using UnityEngine;
public interface IDamageable
{
    void TakeDamage(float damage);
    void KnockBack(UnityEngine.Vector3 impulse);
    void HealDamage(float heal);
    void DropLoot();
    void Death();
    bool isAlive();
    Team GetTeam();
}
