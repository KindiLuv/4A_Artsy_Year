using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    [SerializeField] private WeaponSO _equipedMainWeapon;
    /*TODO 
     [SerializeField] private HeadGearSO _equipedHeadGear;
     [SerializeField] private TorsoSO _equipedTorso;
     [SerializeField] private BootsSO _equipedBoots;
     */
    // Start is called before the first frame update
    public WeaponSO GetMainWeeapon()
    {
        return _equipedMainWeapon;
    }
    
}
