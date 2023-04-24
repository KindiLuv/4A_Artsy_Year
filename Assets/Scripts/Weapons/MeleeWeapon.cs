using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour, IMeleeWeapon
{
    [SerializeField] private MeleeWeaponSO meleeWeaponAttributes;

    public void Start()
    {
        Instantiate(meleeWeaponAttributes.weaponModel, transform);
        
    }


    public int GetWeaponDmg()
    {
        return meleeWeaponAttributes.dmgPerHit;
    }
    
    
}
// MeshCollider weaponCollider = gameObject.AddComponent<MeshCollider>();