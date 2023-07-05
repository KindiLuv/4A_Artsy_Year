using ArtsyNetcode;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : NetEntity
{
    public static EnemyManager instance;
    
    private List<GameObject> enemies = new List<GameObject>();
    
    [SerializeField] private GameObject crate = null;// TODO A supprimer

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            LoadEnemy(1, new Vector3(0.0f, 0f, 25.0f), 3);
            LoadEnemy(0,new Vector3(0.0f,-0.5f,0.0f));
            GameObject o = Instantiate(crate, new Vector3(2.0f, -0.0f, 0.0f),Quaternion.identity);
            o.GetComponent<NetworkObject>().Spawn();
        }
    }
    
    public void LoadEnemy(int enemyId, Vector3 pos, int weaponId = 0)
    {
        enemies.Add(Instantiate(GameRessourceManager.Instance.Enemies[enemyId].Prefab, pos, Quaternion.identity));
        Enemy p = enemies[enemies.Count-1].GetComponent<Enemy>();
        p._enemy = GameRessourceManager.Instance.Enemies[enemyId];
        p.SetupEnemy();
        p.EnemyID = enemyId;
        p.WeaponID = weaponId;
        p.GetComponent<NetworkObject>().Spawn();
    }

    public void InstantiateEnemy(int id,Vector3 pos, WeaponSO weapon = null)
    {
        GameObject enemy = Instantiate(GameRessourceManager.Instance.Enemies[id].Prefab);
        Enemy enemySettings = enemy.GetComponent<Enemy>();
        enemySettings._enemy = GameRessourceManager.Instance.Enemies[id];
        enemySettings.SetupEnemy();
        enemy.transform.position = pos;
        enemy.GetComponent<NetworkObject>().Spawn();
        //LoadDataServerRpc(characterID,weaponID);
        
        /*
        if (weapon != null)
        {
            Instantiate(weapon.weaponModel, enemySettings._enemyHand.transform);
            int enumSize = Enum.GetValues(typeof(WeaponType)).Length;
            for (int i = 0; i < enumSize;i++)
            {
                enemySettings._enemyHand.SetLayerWeight(i+1, 0.0f);
            }
            enemySettings._enemyHand.SetLayerWeight((int)weapon.weaponType+1, 1.0f); 
        }
        */
    }
    
    // [ServerRpc]
    // public void LoadDataServerRpc(int wi)
    // {
    //     LoadDataClientRpc(wi);
    // }
    //
    //
    // [ClientRpc]
    // public void LoadDataClientRpc(int wi)
    // {
    //     LoadData(wi);
    // }
    
    // public void LoadData(int wi)
    // {
    //     foreach (Transform t in _playerHand.transform)
    //     {
    //         Destroy(t.gameObject);
    //     }
    //     if (wi >= 0)
    //     {
    //         _weapon = GameRessourceManager.Instance.Weapons[wi % GameRessourceManager.Instance.Weapons.Count];
    //         if(_weapon != null)
    //         {
    //             Instantiate(_weapon.weaponModel, _playerHand.transform);                
    //         }
    //         int enumSize = Enum.GetValues(typeof(WeaponType)).Length;
    //         for (int i = 0; i < enumSize;i++)
    //         {
    //             _playerHand.SetLayerWeight(i+1, 0.0f);
    //         }
    //         _playerHand.SetLayerWeight((int)_weapon.weaponType+1, 1.0f); 
    //     }
    //     characterID = ci;
    //     weaponID = wi;
    // }
}
