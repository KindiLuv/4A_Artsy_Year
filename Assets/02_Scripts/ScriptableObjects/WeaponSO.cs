using System.Collections.Generic;
using UnityEngine;



public enum WeaponType
{
   Sword,
   GreatSword,
   Axe,
   Mace,
   Bow,
   Staff,   
   Rifle,
   Shotgun,
   Pistol,
   AutoRifle,
   RayGun
}

public enum Rarity
{
   Common,
   Uncommon,
   Rare,
   Epic,
   Legendary,
   Mythic,
   Exotic
}

public enum WeaponFamily
{
   Fish,
   Metal,
   Wood
}

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon/WeaponSO", order = 1)]
public class WeaponSO : ScriptableObject
{
   public string weaponName;
   public WeaponType weaponType;
   public WeaponFamily weaponFamily;
   public Rarity rarity;
   public string weaponDescription;
   public GameObject weaponModel;
   public List<ProjectileSO> projectile;
   public int spawnProjectileAddRandom = 0;
   public int spawnProjectileCount = 1;
   public float spawnProjectileRate = 0.5f;
   public bool autoWeapon = false;
   public GameObject muzzelFlash = null;
   public float muzzelDestroyTime = 1.0f;
   public Vector3 spawnProjectileLocalPosition = Vector3.zero;
   public bool spreadUniform = false;
   public float offsetAngle = 0.0f;   
   public float Angle = 0.0f;
   public float impulseForce = 0.0f;
}
