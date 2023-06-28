using ArtsyNetcode;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : NetEntity
{
    public static EnemyManager instance;
    public List<EnemySO> enemies;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // To call
    public void InstantiateEnemy()
    {
        var enemy = Instantiate(enemies[0].Prefab);
        var enemySettings = enemy.AddComponent<Enemy>();
        enemySettings.enemyInformations = enemies[0];
        enemySettings.SetupEnemy();
    }

    private void Start()
    {
        InstantiateEnemy();
    }
}
