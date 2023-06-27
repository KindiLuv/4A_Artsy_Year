using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRessourceManager : MonoBehaviour
{
    [SerializeField] private List<Biome> biomes = new List<Biome>();
    [SerializeField] private List<CharacterSO> chracters = new List<CharacterSO>();

    private static GameRessourceManager instance = null;

    #region Getter Setter

    public static GameRessourceManager Instance { get { return instance; } }

    public List<CharacterSO> Chracters { get { return chracters; } }

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
