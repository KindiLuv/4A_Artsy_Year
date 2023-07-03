using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRessourceManager : MonoBehaviour
{
    [SerializeField] private List<Biome> biomes = new List<Biome>();
    [SerializeField] private List<CharacterSO> characters = new List<CharacterSO>();
    [SerializeField] private List<WeaponSO> weapons = new List<WeaponSO>();
    [SerializeField] private List<EnemySO> enemies = new List<EnemySO>();

    private static GameRessourceManager instance = null;

    #region Getter Setter

    public static GameRessourceManager Instance { get { return instance; } }

    public List<CharacterSO> Characters { get { return characters; } }
    public List<WeaponSO> Weapons { get { return weapons; } }
    
    public List<EnemySO> Enemies { get { return enemies; } }

    public List<Biome> Biomes { get { return biomes; } }

    #endregion 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


}
