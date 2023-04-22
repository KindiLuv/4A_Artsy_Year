using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon/WeaponSO", order = 1)]
public class WeaponSO : ScriptableObject
{
   public string weaponName;
   public string weaponType;
   public string weaponFamily;
   public string rarity;
   public string weaponDescription;
   public float weaponWeight;
   public int dmgPerHit;
   public GameObject weaponModel;
}
