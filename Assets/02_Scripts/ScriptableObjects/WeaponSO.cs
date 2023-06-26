using System.Collections;
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
   AutoRifle
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
   Metal
}





[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon/WeaponSO", order = 1)]
public class WeaponSO : ScriptableObject
{
   public string weaponName;
   public WeaponType weaponType;
   public WeaponFamily weaponFamily;
   public Rarity rarity;
   public string weaponDescription;
   public float weaponWeight;
   public int dmgPerHit;
   public GameObject weaponModel;
}
