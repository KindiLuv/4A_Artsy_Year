using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    NoAI,
    Base,
    Boss
}

[CreateAssetMenu(fileName = "Enemy", menuName = "Characters/Enemy", order = 1)]
public class EnemySO : ScriptableObject
{
    public EnemyType EnemyType;
    public float BaseHealth;
    public float BaseSpeed;
    public GameObject Prefab;
    public List<WeaponSO> weapons;
}


