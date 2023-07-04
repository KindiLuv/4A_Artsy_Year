using ArtsyNetcode;
using Unity.Netcode;
using UnityEngine;

public class DestroyObject : NetEntity, IDamageable
{
    [SerializeField] private float _health = 10.0f;
    [SerializeField] private GameObject effectDestroy = null;
    public void TakeDamage(float damage)
    {
        _health -= damage;
        if(_health <= 0)
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

    }

    public void Death()
    {
        if (IsServer)
        {
            DropLoot();
            GetComponent<NetworkObject>().Despawn();            
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (effectDestroy != null)
        {
            Destroy(Instantiate(effectDestroy, transform.position, transform.rotation), 1.0f);
        }
        Destroy(gameObject);
    }

    public bool isAlive()
    {
        return _health > 0;
    }

    public Team GetTeam()
    {
        return Team.Object;
    }
}
