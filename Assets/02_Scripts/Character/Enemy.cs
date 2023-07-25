using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.NetCode;
using Unity.Netcode;
using UnityEngine;

public class Enemy : Character
{
    protected int enemyID = -1;
    protected int _currentWeapon = -1;
    public EnemySO _enemy;
    protected List<WeaponSO> _weapons = new List<WeaponSO>();
    protected Animator _animator;
    protected CapsuleCollider _enemyCollider;
    
    [SerializeField] private Animator _enemyHand;
    [SerializeField] private GameObject enemyModelSpawn = null;
    
    protected static readonly int Attacking = Animator.StringToHash("Attacking");
    protected static readonly int ASpeed = Animator.StringToHash("Speed");

    protected float contactTimer = 1f;
    #region Getter Setter

    public int EnemyID { get { return enemyID; } set { enemyID = value; } }
    public List<WeaponSO> Weapon { get { return _weapons; } }
    
    #endregion

    protected override void Awake()
    {
        base.Awake();
        team = Team.Enemy;
    }

    protected virtual void Start()
    {        
        if (GameNetworkManager.IsOffline)
        {
            LoadData(EnemyID);
        }        
    }

    [ClientRpc]
    public void LoadDataClientRpc(int ei)
    {
        LoadData(ei);
    }

    public override void DropLoot()
    {
        base.DropLoot();
        if(IsServer && _enemy != null)
        {
            foreach(LootableSO ls in _enemy.lootables)
            {
                if(ls.SpawnProbabilite > UnityEngine.Random.Range(0.0f,100.0f))
                {
                    int nb = UnityEngine.Random.Range(ls.Spawn, ls.Spawn + ls.SpawnAddRandom);
                    if (nb > 0)
                    {
                        InstantiateLootClientRpc(GameRessourceManager.Instance.GetIdByLoot(ls), nb);
                    }
                }
            }
        }
    }

    [ClientRpc]
    public void InstantiateLootClientRpc(int id,int nb)
    {
        GameObject lp = GameRessourceManager.Instance.LootParticlePrefab;
        LootableSO ls = GameRessourceManager.Instance.Loots[id];
        GameObject obj = Instantiate(lp,transform.position,Quaternion.identity);
        Loot l = obj.AddComponent<Loot>();
        l.initLoot(ls,nb);
    }

    public void LoadData(int ei)
    {
        foreach(Transform t in enemyModelSpawn.transform)
        {
            Destroy(t.gameObject);
        }
        if (ei >= 0)
        {
            _enemy = GameRessourceManager.Instance.Enemies[ei% GameRessourceManager.Instance.Enemies.Count];
            if (_enemy != null)
            {
                Instantiate(_enemy.Prefab, enemyModelSpawn.transform.position, enemyModelSpawn.transform.rotation, enemyModelSpawn.transform);
                _enemyCollider = GetComponent<CapsuleCollider>();
                _enemyCollider.radius *= _enemy.size;
                _animator = GetComponentInChildren<Animator>();
                if (!GameNetworkManager.IsOffline)
                {
                    SetupEnemy();
                }
            }
        }
        foreach (Transform t in _enemyHand.transform)
        {
            Destroy(t.gameObject);
        }

        for (int i = 0; i < _weapons.Count; i++)
        {
            
        }
        if (_weapons.Count > 0)
        {
            if(_weapons[0] != null)
            {
                if (_weapons[0].weaponModel != null)
                {
                    Instantiate(_weapons[0].weaponModel, _enemyHand.transform);
                }
                _currentWeapon = 0;
            }
            int enumSize = Enum.GetValues(typeof(WeaponType)).Length;
            for (int i = 0; i < enumSize;i++)
            {
                _enemyHand.SetLayerWeight(i+1, 0.0f);
            }
            _enemyHand.SetLayerWeight((int)_weapons[0].weaponType+1, 1.0f); 
        }
        enemyID = ei;
        
    }

    public void SetupEnemy()
    {        
        _maxHealth = _enemy.BaseHealth;
        _health = _enemy.BaseHealth;
        _prefab = _enemy.Prefab;
        _speed = _enemy.BaseSpeed;
        _weapons = _enemy.weapons;
    }
    

    [ClientRpc]
    public void BasicAttackClientRpc(Vector3 pos, Quaternion rot,double timeStamp, ClientRpcParams clientRpcParams = default)
    {
        BasicAttack(pos, rot, (float)timeStamp);       
    }

    public void BasicAttack(Vector3 pos, Quaternion rot,float time,bool serveur = false)
    {
        _enemyHand.SetTrigger(Attacking);

        if (!serveur && IsClient || serveur && !IsClient)
        {
            ProjectileManager.Instance.SpawnProjectile(_weapons[_currentWeapon], pos, rot, Team.Enemy,((float)NetworkManager.Singleton.LocalTime.Time) - time);
        }
    }

    public void ChangeWeapon(int i)
    {
        _currentWeapon = (_currentWeapon+i) % _weapons.Count;
    }
}
