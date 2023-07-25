using System.Collections.Generic;
using UnityEngine;

public class GameRessourceManager : MonoBehaviour
{
    [SerializeField] private List<Biome> biomes = new List<Biome>();
    [SerializeField] private List<CharacterSO> characters = new List<CharacterSO>();
    [SerializeField] private List<WeaponSO> weapons = new List<WeaponSO>();
    [SerializeField] private List<EnemySO> enemies = new List<EnemySO>();
    [SerializeField] private List<LootableSO> loots = new List<LootableSO>();
    [SerializeField] private List<GameObject> enemyTypePrebab = new List<GameObject>();
    [SerializeField] private GameObject lootParticlePrefab = null;
    private static GameRessourceManager instance = null;

    #region Getter Setter

    public static GameRessourceManager Instance { get { return instance; } }

    public List<CharacterSO> Characters { get { return characters; } }
    public List<WeaponSO> Weapons { get { return weapons; } }

    public List<EnemySO> Enemies { get { return enemies; } }
    public List<LootableSO> Loots { get { return loots; } }
    public GameObject LootParticlePrefab { get { return lootParticlePrefab; } }
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

    public int GetIdByLoot(LootableSO ls)
    {
        for (int i = 0; i < loots.Count; i++)
        {
            if (ls == loots[i])
            {
                return i;
            }
        }
        return -1;
    }

    public int GetIdByWeapon(WeaponSO ws)
    {
        for(int i = 0; i  < weapons.Count; i++)
        {
            if (ws == weapons[i])
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
