using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnableObject", menuName = "ScriptableObjects/SpawnableObject", order = 1)]
public class SpawnableObject : ScriptableObject
{
    [SerializeField] private int spawnID = 0;
    [SerializeField] private GameObject prefab = null;
    [SerializeField] private bool isNetCodeObject = false;//spawn Only on Server

    #region Getter Setter
    public int SpawnID { get { return spawnID; } }
    public GameObject Prefab { get { return prefab; } }
    #endregion
}
