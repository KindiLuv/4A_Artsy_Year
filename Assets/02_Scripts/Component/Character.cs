using System.Collections;
using UnityEngine;
using ArtsyNetcode;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class Character : NetEntity, IDamageable
{
    protected bool _ded;
    protected float _health;
    protected bool _isInvicible;
    protected float _maxHealth;
    protected float _speed;
    protected GameObject _prefab;

    public void TakeDamage(float damage)
    {
        if (_ded)
        {
            return;
        }
        // TODO radiant couleur en fonction degats
        UIManager.instance.CreateFloatingText(damage.ToString(), transform.position + Vector3.up, Color.red);
        if (_health - damage < 1)
        {
            Death();
            return;
        }
        _health -= damage;
    }

    public void HealDamage(float heal)
    {
        if (_health + heal > _maxHealth)
        {
            _health = _maxHealth;
            return;
        }
        _health += heal;
    }

    public void DropLoot()
    {
        throw new System.NotImplementedException();
    }

    public void Death()
    {
        Debug.Log("You died");
        _ded = true;
    }

    public bool isAlive()
    {
        return !_ded;
    }

    public virtual void Teleportation(Vector3 positionTarget)
    {
        transform.position = positionTarget;
    }
}
