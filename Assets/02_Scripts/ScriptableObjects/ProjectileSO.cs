using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveProjectileType
{
    Static,
    Linear,
    Sin
}

public enum ShapeProjectileType
{
    Sphere,
    Box,
    Raycast,
    ArcSphere
}

[CreateAssetMenu(fileName = "Projectile", menuName = "ScriptableObjects/Weapon/ProjectileSO", order = 4)]
public class ProjectileSO : ScriptableObject
{
    public MoveProjectileType moveProjectileType = MoveProjectileType.Linear;
    [DrawIf("moveProjectileType", MoveProjectileType.Sin, ComparisonType.Equals)] public float sinForce = 1.0f;
    public float speed = 1.0f;
    public List<GameObject> prefab;
    public ShapeProjectileType shapeProjectileType = ShapeProjectileType.Sphere;
    public Vector3 collideOffset = Vector3.zero;
    [DrawIf("shapeProjectileType", ShapeProjectileType.Sphere,ComparisonType.Equals)] public float radius = 1.0f;
    [DrawIf("shapeProjectileType", ShapeProjectileType.Box, ComparisonType.Equals)] public Vector3 extends;
    [DrawIf("shapeProjectileType", ShapeProjectileType.Raycast, ComparisonType.Equals)] public float rayDistance = 100.0f;
    [DrawIf("shapeProjectileType", ShapeProjectileType.ArcSphere, ComparisonType.Equals)] public float arcRadius = 1.0f;
    public float lifeTime = 20.0f;
    public int wallBounds = 0;
    public GameObject explodeEffect;
    public float timeExplode = 2.0f;
}