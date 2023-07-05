using System.Collections;
using UnityEngine;
using ArtsyNetcode;
using Unity.Netcode;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.AI;
using Assets.Scripts.NetCode;

public enum Team
{
    Player,
    PlayerFF,
    Enemy,
    PNJ,
    Object
}

/*[RequireComponent(typeof(CharacterController))]*/
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
    protected float healSameTime = 0.0f;
    private float timeDamage = 0.0f;
    private float timeHeal = 0.0f;
    [SerializeField] protected Team team = Team.PNJ;
    private Renderer[] renderers;
    private bool visualDamage = false;
    private float timeVisualDamage = 0.0f;
    protected Vector3 lastGroundPosition = Vector3.zero;
    #region Getter Setter

    public bool Ded { get { return _ded; } }
    public float Health { get { return _health; } }
    public bool IsInvicible { get { return _isInvicible; } }
    public float MaxHealth { get { return _maxHealth; } }
    public float Speed { get { return _speed; } }

    public Vector3 LastGroundPosition { get { return lastGroundPosition; } }

    public virtual bool AtionLocked { set { _actionLocked = value; } get { return _actionLocked; } }

    public Team Team { get { return team; } }

    public Team GetTeam() { return team; } 

    #endregion

    protected override void Awake()
    {
        base.Awake();
        gameObject.layer = 8;
        renderers = GetComponentsInChildren<MeshRenderer>();
    }    

    public void DamageIndicator()
    {
        timeDamage -= Time.deltaTime;
        if (timeDamage <= 0.0f && damageSameTime > 0.0f)
        {
            Vector3 pos = transform.position + Vector3.up * 3.0f;
            UIManager.instance.CreateFloatingText(damageSameTime.ToString(), pos, Color.red, new Color(1.0f, 0.5f, 0.0f));
            StartCoroutine(ApplyVisualDamage());
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(x => x != NetworkManager.Singleton.LocalClientId).ToList() }
            };
            CreateFTClientRpc(damageSameTime, pos, clientRpcParams);
            timeDamage = 0.15f;
            damageSameTime = 0.0f;
        }
    }

    public void HealIndicator()
    {
        timeHeal -= Time.deltaTime;
        if (timeHeal <= 0.0f && healSameTime > 0.0f)
        {
            Vector3 pos = transform.position + Vector3.up * 3.0f;
            UIManager.instance.CreateFloatingText(healSameTime.ToString(), pos, Color.green, new Color(0.0f, 1.0f, 0.5f));
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(x => x != NetworkManager.Singleton.LocalClientId).ToList() }
            };
            CreateFTHealClientRpc(healSameTime, pos, clientRpcParams);
            timeHeal = 0.15f;
            healSameTime = 0.0f;
        }
    }

    protected virtual void Update()
    {
        if (GameNetworkManager.IsOffline || IsServer)
        {
            HealIndicator();
            DamageIndicator();
        }
        lastGroundPosition = transform.position;
    }

    [ClientRpc]
    public void CreateFTClientRpc(float damage,Vector3 pos,ClientRpcParams clientRpcParams = default)
    {
        UIManager.instance.CreateFloatingText(damage.ToString(), pos, Color.red, new Color(1.0f, 0.5f, 0.0f));
        StartCoroutine(ApplyVisualDamage());
    }

    [ClientRpc]
    public void CreateFTHealClientRpc(float damage, Vector3 pos, ClientRpcParams clientRpcParams = default)
    {
        UIManager.instance.CreateFloatingText(damage.ToString(), pos, Color.green, new Color(0.0f, 1.0f, 0.5f));
    }

    public IEnumerator ApplyVisualDamage()
    {
        timeVisualDamage = 0.15f;
        if (!visualDamage)
        {
            visualDamage = true;
            List<Color> colors = new List<Color>();
            List<Texture> texture = new List<Texture>();
            List<bool> baseEnabled = new List<bool>();
            int ec = Shader.PropertyToID("_EmissionColor");
            int em = Shader.PropertyToID("_EmissionMap");
            List<Material> mats = new List<Material>();
            Color col = new Color(1.0f, 0.5f, 0.5f);
            foreach (Renderer r in renderers)
            {
                if (r != null)
                {
                    foreach (Material m in r.materials)
                    {
                        if (m.HasProperty(ec) && m.HasProperty(em))
                        {
                            texture.Add(m.GetTexture(em));
                            m.SetTexture(em, null);
                            colors.Add(m.GetColor(ec));
                            baseEnabled.Add(m.IsKeywordEnabled("_EMISSION"));
                            m.EnableKeyword("_EMISSION");
                            m.SetColor(ec, col);
                            mats.Add(m);
                        }
                    }
                }
            }
            yield return null;
            while (timeVisualDamage >= 0.0f)
            {
                timeVisualDamage -= Time.deltaTime;
                for(int i = 0; i < mats.Count;i++)
                {
                    mats[i].SetColor(ec, col * (timeVisualDamage/0.15f));
                }
                yield return null;
            }
            for (int i = 0; i < mats.Count; i++)
            {
                mats[i].SetTexture(em, texture[i]);
                mats[i].SetColor(ec, colors[i]);
                if (!baseEnabled[i])
                {
                    mats[i].DisableKeyword("_EMISSION");
                }
            }
            visualDamage = false;
        }
        yield return null;
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
            _health = 0;
            Death();
            return;
        }
        _health -= damage;
    }

    public virtual void KnockBack(Vector3 impulse)
    {

    }

    public void HealDamage(float heal)
    {
        if (_health + heal > _maxHealth)
        {
            healSameTime += _maxHealth - _health;
            _health = _maxHealth;            
            return;
        }
        healSameTime += heal;
        _health += heal;
    }

    public void DropLoot()
    {
        throw new System.NotImplementedException();
    }

    public virtual void Death()
    {
        Debug.Log($"{gameObject.name} died");
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
