using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveProjectileType
{
    Linear,
    Sin
}

[CreateAssetMenu(fileName = "Projectile", menuName = "ScriptableObjects/Weapon/ProjectileSO", order = 4)]
public class ProjectileSO : ScriptableObject
{
    public MoveProjectileType moveProjectileType = MoveProjectileType.Linear;
    public float speed = 1.0f;
    public List<GameObject> prefab;    
}