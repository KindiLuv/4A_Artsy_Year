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
    [SerializeField] private float maxLife;
    [SerializeField] private float maxShield;//+ tards
    [SerializeField] private float maxMana;//+ tards
    [SerializeField] private float maxSpeed = 1.0f;

    #region Getter Setter

    public CharacterClass CharacterClass { get { return characterClass; } }
    public GameObject Prefab { get { return prefab; } }
    public float MaxLife { get { return maxLife; } }
    public float MaxShield { get { return maxShield; } }
    public float MaxMana { get { return maxMana; } }
    public float MaxSpeed { get { return maxSpeed; } }

    #endregion
}
