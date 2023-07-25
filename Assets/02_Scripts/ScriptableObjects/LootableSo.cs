using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon/WeaponSO", order = 1)]
public class LootableSo : ScriptableObject
{
    [SerializeField] private Mesh m;
    //[SerializeField] private float rand
}
