using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome", menuName = "ScriptableObjects/Biome", order = 2)]
public class Biome : ScriptableObject
{
    [SerializeField] private int biomeID = 0;
    [SerializeField] private Material ground = null;
    [SerializeField] private Material wall = null;
    [SerializeField] private Material ceil = null;

    #region Getter Setter

    public int BiomeID { get { return biomeID; } }
    public Material Ground { get { return ground; } }
    public Material Wall { get { return wall; } }
    public Material Ceil { get { return ceil; } }

    #endregion
}
