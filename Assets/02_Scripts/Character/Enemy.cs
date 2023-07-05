using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.NetCode;
using Unity.Netcode;
using UnityEngine;

public class Enemy : Character
{
    private int enemyID = -1;
    private int weaponID = -1;
    public EnemySO _enemy;
    protected List<WeaponSO> _weapons = new List<WeaponSO>();
    
    [SerializeField] private Animator _enemyHand;
    [SerializeField] private GameObject enemyModelSpawn = null;
    
    private static readonly int Attacking = Animator.StringToHash("Attacking");
    #region Getter Setter

    public int EnemyID { get { return enemyID; } set { enemyID = value; } }
    public int WeaponID { get { return weaponID; } set { weaponID = value; } }
    public List<WeaponSO> Weapon { get { return _weapons; } }
    
    #endregion

    protected override void Awake()
    {
        base.Awake();
        team = Team.Enemy;
    }

    protected void Start()
    {
        if (GameNetworkManager.IsOffline)
        {
            LoadData(EnemyID, WeaponID);
        }
    }
    
    public override void OnNetworkSpawn()
    {
        LoadDataClientRpc(EnemyID,WeaponID);
    }
    
    [ServerRpc]
    public void LoadDataServerRpc(int ci,int wi)
    {
        LoadDataClientRpc(ci, wi);
    }

    [ClientRpc]
    public void LoadDataClientRpc(int ci,int wi)
    {
        LoadData(ci, wi);
    }
    
    public void LoadData(int ei, int wi)
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
                //Instantiate(_enemy.Prefab, enemyModelSpawn.transform.position, enemyModelSpawn.transform.rotation, enemyModelSpawn.transform);
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
        if (wi >= 0)
        {
            //_weapons.Add(GameRessourceManager.Instance.Weapons[wi % GameRessourceManager.Instance.Weapons.Count]);
            if(_weapons[0] != null)
            {
                Instantiate(_weapons[0].weaponModel, _enemyHand.transform);                
            }
            int enumSize = Enum.GetValues(typeof(WeaponType)).Length;
            for (int i = 0; i < enumSize;i++)
            {
                _enemyHand.SetLayerWeight(i+1, 0.0f);
            }
            _enemyHand.SetLayerWeight((int)_weapons[0].weaponType+1, 1.0f); 
        }
        enemyID = ei;
        weaponID = wi;
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

    // TODO demander a tipot si c bon
    public void BasicAttack(Vector3 pos, Quaternion rot,float time,bool serveur = false)
    {
        _enemyHand.SetTrigger(Attacking);

        if (!serveur && IsClient || serveur && !IsClient)
        {
            ProjectileManager.Instance.SpawnProjectile(_weapons[0], pos, rot, Team.Enemy,((float)NetworkManager.Singleton.LocalTime.Time) - time);
        }
    }
    
}
