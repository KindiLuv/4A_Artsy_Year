using UnityEngine;

[CreateAssetMenu(fileName = "DistanceWeapon", menuName = "ScriptableObjects/Weapon/DistanceWeaponSO", order = 3)]
public class DistanceWeaponSO : WeaponSO
{
    public int bulletRange;
    public int bulletPerShot;
    public string bulletType;
    public float delayPostShot;
}
