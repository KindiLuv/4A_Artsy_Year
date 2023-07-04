using ArtsyNetcode;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyManager : NetEntity
{
    public static EnemyManager instance;

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
            InstantiateEnemy(1, new Vector3(0.0f, -0.5f, 20.0f));
            InstantiateEnemy(0,new Vector3(0.0f,-0.5f,0.0f));
            GameObject o = Instantiate(crate, new Vector3(2.0f, -0.0f, 0.0f),Quaternion.identity);
            o.GetComponent<NetworkObject>().Spawn();
        }
    }

    public void InstantiateEnemy(int id,Vector3 pos)
    {
        GameObject enemy = Instantiate(GameRessourceManager.Instance.Enemies[id].Prefab);
        Enemy enemySettings = enemy.GetComponent<Enemy>();
        enemySettings.enemyInformations = GameRessourceManager.Instance.Enemies[id];
        enemySettings.SetupEnemy();
        enemy.transform.position = pos;
        enemy.GetComponent<NetworkObject>().Spawn();
    }
}
