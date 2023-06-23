using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    public EnemySO enemyInformations;

    public void SetupEnemy()
    {
        _maxHealth = enemyInformations.BaseHealth;
        _health = enemyInformations.BaseHealth;
        _prefab = enemyInformations.Prefab;
        _speed = enemyInformations.BaseSpeed;
    }
    
}