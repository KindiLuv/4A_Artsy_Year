using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Enemy : Character
{
    public EnemySO enemyInformations;
    protected List<WeaponSO> _weapons;
    public Animator _enemyHand;
    private static readonly int Attacking = Animator.StringToHash("Attacking");

    protected override void Awake()
    {
        base.Awake();
        team = Team.Enemy;
    }

    public void SetupEnemy()
    {        
        _maxHealth = enemyInformations.BaseHealth;
        _health = enemyInformations.BaseHealth;
        _prefab = enemyInformations.Prefab;
        _speed = enemyInformations.BaseSpeed;
        _weapons = enemyInformations.weapons;
    }
    
    [ServerRpc]
    public void BasicAttackServerRpc(Vector3 pos, Quaternion rot,ulong localClientId,double timeStamp)
    {        
        BasicAttack(pos,rot,(float)timeStamp,true);
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(x => x != localClientId).ToList() }
        };
        BasicAttackClientRpc(pos, rot, timeStamp,clientRpcParams);
    }

    [ClientRpc]
    public void BasicAttackClientRpc(Vector3 pos, Quaternion rot,double timeStamp, ClientRpcParams clientRpcParams = default)
    {
        BasicAttack(pos, rot, (float)timeStamp);       
    }

    // TODO demander a tipot si c bon
    public void BasicAttack(Vector3 pos, Quaternion rot,float time,bool serveur = false)
    {
        _enemyHand.SetTrigger(Attacking);
        if (IsLocalPlayer && !serveur)
        {
            BasicAttackServerRpc(pos,rot,NetworkManager.Singleton.LocalClientId, NetworkManager.Singleton.LocalTime.Time);
        }
        if (!serveur && IsClient || serveur && !IsClient)
        {
            ProjectileManager.Instance.SpawnProjectile(_weapons[0], pos, rot, Team.Enemy,((float)NetworkManager.Singleton.LocalTime.Time) - time);
        }
    }
}
