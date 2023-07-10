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
    
    [SerializeField] private Animator _enemyHand;
    [SerializeField] private GameObject enemyModelSpawn = null;

    protected static readonly int Attacking = Animator.StringToHash("Attacking");
    protected static readonly int ASpeed = Animator.StringToHash("Speed");
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
                if (!GameNetworkManager.IsOffline)
                {
                    SetupEnemy();
                }
                _animator = GetComponentInChildren<Animator>();
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
                Instantiate(_weapons[0].weaponModel, _enemyHand.transform);
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
            ProjectileManager.Instance.SpawnProjectile(_weapons[0], pos, rot, Team.Enemy,((float)NetworkManager.Singleton.LocalTime.Time) - time);
        }
    }   
}
