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

    [SerializeField] private List<GameObject> _enemyTypePrebab;

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
            LoadEnemy(1, new Vector3(0.0f, 0f, 25.0f));
            LoadEnemy(0,new Vector3(0.0f,-0.5f,0.0f));
            GameObject o = Instantiate(crate, new Vector3(2.0f, -0.0f, 0.0f),Quaternion.identity);
            o.GetComponent<NetworkObject>().Spawn();
        }
    }
    
    public void LoadEnemy(int enemyId, Vector3 pos)
    {
        enemies.Add(Instantiate(_enemyTypePrebab[(int)GameRessourceManager.Instance.Enemies[enemyId].EnemyType], pos, Quaternion.identity));
        Enemy p = enemies[enemies.Count-1].GetComponent<Enemy>();
        p._enemy = GameRessourceManager.Instance.Enemies[enemyId];
        p.SetupEnemy();
        p.GetComponent<NetworkObject>().Spawn();
        p.LoadDataClientRpc(enemyId);
    }
}
