using System.Collections;
using UnityEngine;
using ArtsyNetcode;
using TMPro;
using Unity.Netcode;
using System.Linq;

public enum Team
{
    Player,
    PlayerFF,
    Enemy,
    PNJ
}

[RequireComponent(typeof(CharacterController))]
public class Character : NetEntity, IDamageable
{
    [SerializeField] protected bool _ded;
    [SerializeField] protected float _health;
    [SerializeField] protected bool _isInvicible;
    [SerializeField] protected float _maxHealth;
    [SerializeField] protected float _speed;
    protected GameObject _prefab;
    protected bool _actionLocked = false;
    protected float damageSameTime = 0.0f;
    private float timeDamage = 0.0f;
    [SerializeField] protected Team team = Team.PNJ;
    #region Getter Setter

    public virtual bool AtionLocked { set { _actionLocked = value; } get { return _actionLocked; } }

    public Team Team { get { return team; } }

    #endregion

    protected override void Awake()
    {
        base.Awake();
        gameObject.layer = 8;
    }    

    public virtual void Update()
    {
        timeDamage -= Time.deltaTime;
        if (timeDamage <= 0.0f && damageSameTime > 0.0f)
        {
            Vector3 pos = transform.position + Vector3.up * 3.0f;
            UIManager.instance.CreateFloatingText(damageSameTime.ToString(), pos, Color.red,new Color(1.0f,0.5f,0.0f));
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(x => x != NetworkManager.Singleton.LocalClientId).ToList() }
            };
            CreateFTClientRpc(damageSameTime, pos,clientRpcParams);
            timeDamage = 0.15f;
            damageSameTime = 0.0f;
        }
    }

    [ClientRpc]
    public void CreateFTClientRpc(float damage,Vector3 pos,ClientRpcParams clientRpcParams = default)
    {
        UIManager.instance.CreateFloatingText(damage.ToString(), pos, Color.red, new Color(1.0f, 0.5f, 0.0f));
    }

    public virtual void TakeDamage(float damage)
    {
        if (_ded)
        {
            return;
        }
        damageSameTime += damage;
        // TODO radiant couleur en fonction degats        
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
