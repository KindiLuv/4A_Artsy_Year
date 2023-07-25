using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loot", menuName = "ScriptableObjects/Loot", order = 4)]
public class LootableSO : ScriptableObject
{
    [SerializeField] private Mesh m;
    [SerializeField] private Texture2D mainText;
    [SerializeField] private float rangeTake = 5.0f;
    [SerializeField] private int spawn = 1;
    [SerializeField] private int spawnAddRandom = 0;
    [SerializeField] private float offsetHeight = 0.0f;
    [SerializeField] [Range(0.0f,100.0f)] private float spawnProbabilite = 0;

    #region Getter Setter

    public Mesh Mesh { get { return m; } }
    public float RangeTake { get { return rangeTake; } }
    public int Spawn { get { return spawn; } }
    public int SpawnAddRandom { get { return spawnAddRandom; } }
    public float SpawnProbabilite { get { return spawnProbabilite; } }
    public float OffsetHeight { get { return offsetHeight; } }

    public Texture2D MainText { get { return mainText; } }
    #endregion
}
