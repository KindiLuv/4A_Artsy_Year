using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome", menuName = "ScriptableObjects/Biome", order = 2)]
public class Biome : ScriptableObject
{
    [SerializeField] private AudioClip biomeMusic;
    [SerializeField] private AudioClip biomeAmbient;
    [SerializeField] private int biomeID = 0;
    [SerializeField] private Material ground = null;
    [SerializeField] private Material wall = null;
    [SerializeField] private Material ceil = null;
    [SerializeField] private Material border = null;
    [SerializeField] private Material borderHole = null;
    [SerializeField] private Material voidHole = null;
    [SerializeField] private SpawnableObject door = null;
    [SerializeField] private SpawnableObject bossDoor = null;
    [SerializeField] private List<SpawnableObject> spawnObjects = new List<SpawnableObject>();
    [SerializeField][Range(0, 100)] private int randomFillPercent = 46;
    [SerializeField][Range(0, 100)] private int randomAddPercent = 4;
    [SerializeField][Range(0.0f, 100.0f)] private double hole_rate_chunck = 0.0f;
    [SerializeField][Range(0, 100)] private int hole_fill_percent = 0;
    [SerializeField] private int sunKelvin = 8100;
    [SerializeField] private float sunIntensity = 0.5f;
    [SerializeField] private Vector2 speedSunMove;
    [SerializeField] private Vector2 sunSizeCookie;
    [SerializeField] private Texture2D sunLightCookie;    

    #region Getter Setter
    public AudioClip BiomeMusic { get { return biomeMusic; } }
    public int BiomeID { get { return biomeID; } }
    public Material Ground { get { return ground; } }
    public Material Wall { get { return wall; } }
    public Material Ceil { get { return ceil; } }
    public Material Border { get { return border; } }
    public Material BorderHole { get { return borderHole; } }
    public Material VoidHole { get { return voidHole; } }
    public SpawnableObject Door { get { return door; } }
    public SpawnableObject BossDoor { get { return bossDoor; } }
    public List<SpawnableObject> SpawnObjects { get { return spawnObjects; } }
    public int RandomFillPercent { get { return randomFillPercent; } }
    public int RandomAddPercent { get { return randomAddPercent; } }
    public double HoldeRateChunck { get { return hole_rate_chunck; } }
    public int HoleFill { get { return hole_fill_percent; } }
    public int SunKelvin { get { return sunKelvin; } }
    public float SunIntensity { get { return sunIntensity; } }
    public Vector2 SpeedSunMove { get { return speedSunMove; } }
    public Vector2 SunSizeCookie { get { return sunSizeCookie; } }    
    public Texture2D SunLightCookie { get { return sunLightCookie; } }
    #endregion
}
