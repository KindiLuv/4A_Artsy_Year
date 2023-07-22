using UnityEngine;

public enum CharacterClass
{
    Paladin,
    Rogue,
    Druid,
    Cleric,
    Wizard,
    Bard,
    Assassin,
    Hunter,
    Berserker,
    Necromancer,
    Summoner,
    Vampire,
    Elf,
}


[CreateAssetMenu(fileName = "CharacterSO", menuName = "ScriptableObjects/character/CharacterSO", order = 2)]
public class CharacterSO : ScriptableObject
{
    [SerializeField] private CharacterClass characterClass = CharacterClass.Paladin;
    [SerializeField] private GameObject prefab = null;
    [SerializeField] private float baseLife;
    [SerializeField] private float baseShield;//+ tards
    [SerializeField] private float baseMana;//+ tards
    [SerializeField] private float baseSpeed = 1.0f;
    [SerializeField] private WeaponSO baseWeapon;

    #region Getter Setter

    public CharacterClass CharacterClass { get { return characterClass; } }
    public GameObject Prefab { get { return prefab; } }
    public float BaseLife { get { return baseLife; } }
    public float BaseShield { get { return baseShield; } }
    public float BaseMana { get { return baseMana; } }
    public float BaseSpeed { get { return baseSpeed; } }
    public WeaponSO Weapon { get { return baseWeapon; } }

    #endregion
}
