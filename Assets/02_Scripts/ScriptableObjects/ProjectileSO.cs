using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "ScriptableObjects/Weapon/ProjectileSO", order = 4)]
public class ProjectileSO : ScriptableObject
{
    public float speed = 1.0f;
    public List<GameObject> prefab;
}