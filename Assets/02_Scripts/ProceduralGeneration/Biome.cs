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
    [SerializeField] private Material border = null;
    [SerializeField][Range(0.0f, 100.0f)] private double hole_rate_chunck = 0.0f;
    [SerializeField][Range(0, 100)] private int hole_fill_percent = 0;

    #region Getter Setter

    public int BiomeID { get { return biomeID; } }
    public Material Ground { get { return ground; } }
    public Material Wall { get { return wall; } }
    public Material Ceil { get { return ceil; } }
    public Material Border { get { return border; } }
    public double HoldeRateChunck { get { return hole_rate_chunck; } }
    public int HoleFill { get { return hole_fill_percent; } }

    #endregion
}
