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
    public float weaponChangeRate = 30.0f;
    public float maxFollowDistance = 15.0f;
    public float minDistanceTarget = 5.0f;
    public Vector3 randomMin;
    public Vector3 randomMax;
    public float timeSmoothRandom = 1.0f;
    public float idelRandomSphereInsideUnits = 0.0f;
    public float minTimeRandomSIU = 2.0f;
    public float maxTimeRandomSIU = 5.0f;
    public float timeLostPlayer = 2.0f;
    public GameObject effectDie = null;
}


