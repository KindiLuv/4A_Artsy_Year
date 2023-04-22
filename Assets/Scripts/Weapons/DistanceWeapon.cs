using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceWeapon : MonoBehaviour, IDistanceWeapon
{
    [SerializeField] private DistanceWeaponSO distanceWeaponAttributes;
    
    private bool _isEquiped = false;
    
    public void EquipWeapon()
    {
        _isEquiped = true;
    }

    public void DesequipWeapon()
    {
        _isEquiped = false;
    }

    public bool GetEquipState()
    {
        return _isEquiped;
    }

    public void RegisterInDex()
    {
        Debug.Log("REGISTER IN DEX");
    }

    public void InflictDamage(int damage)
    {
        
    }

    public void Shoot()
    {
        
    }

    public void GetBulletRange()
    {
        
    }
    
    public int GetWeaponDmg()
    {
        return 0;
    }


}
