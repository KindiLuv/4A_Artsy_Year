using System.Collections.Generic;
using UnityEngine;

public class GameRessourceManager : MonoBehaviour
{
    [SerializeField] private List<Biome> biomes = new List<Biome>();
    [SerializeField] private List<CharacterSO> characters = new List<CharacterSO>();
    [SerializeField] private List<WeaponSO> weapons = new List<WeaponSO>();
    [SerializeField] private List<EnemySO> enemies = new List<EnemySO>();
    [SerializeField] private List<GameObject> enemyTypePrebab = new List<GameObject>();

    private static GameRessourceManager instance = null;

    #region Getter Setter

    public static GameRessourceManager Instance { get { return instance; } }

    public List<CharacterSO> Characters { get { return characters; } }
    public List<WeaponSO> Weapons { get { return weapons; } }
    
    public List<EnemySO> Enemies { get { return enemies; } }

    public List<Biome> Biomes { get { return biomes; } }
    public List<GameObject> EnemyTypePrebab { get { return enemyTypePrebab; } }

    #endregion 


    public int GetIdByEnemy(EnemySO es)
    {
        for(int i = 0; i  < enemies.Count; i++)
        {
            if (es == enemies[i])
            {
                return i;
            }
        }
        return -1;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


}
