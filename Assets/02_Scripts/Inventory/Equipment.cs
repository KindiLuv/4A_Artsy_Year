using UnityEngine;

public class Equipment
{
    [SerializeField] private WeaponSO _equipedMainWeapon;
    /*TODO 
     [SerializeField] private HeadGearSO _equippedHeadGear;
     [SerializeField] private TorsoSO _equippedTorso;
     [SerializeField] private BootsSO _equippedBoots;
     */
    // Start is called before the first frame update
    public WeaponSO GetMainWeeapon()
    {
        return _equipedMainWeapon;
    }
    
}
