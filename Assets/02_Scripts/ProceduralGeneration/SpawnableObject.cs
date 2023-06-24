using System.Collections.Generic;
using UnityEngine;

public enum RotateMode
{
    No,
    Zero,
    TwoRightAngleRandom,
    FoorRightAngleRandom,
    RandomBetweenTwoAngle
}

[CreateAssetMenu(fileName = "SpawnableObject", menuName = "ScriptableObjects/SpawnableObject", order = 1)]
public class SpawnableObject : ScriptableObject
{
    [SerializeField] private int spawnID = 0;
    [SerializeField] private List<GameObject> prefab = null;
    [SerializeField] private Vector2Int size;
    [SerializeField] private bool isNetCodeObject = false;
    [SerializeField] private bool wallObject = false;
    [SerializeField] private RotateMode rotateMode = RotateMode.Zero;
    [DrawIf("rotateMode", RotateMode.RandomBetweenTwoAngle, ComparisonType.Equals)]
    [SerializeField] private float angleMin = 0.0f;
    [DrawIf("rotateMode", RotateMode.RandomBetweenTwoAngle, ComparisonType.Equals)]
    [SerializeField] private float angle = 0.0f;
    [SerializeField] [Range(0, 100)] float spawnRateChunck = 50.0f;
    [SerializeField] private bool hasSpawnFillPercent = false;
    [DrawIf("hasSpawnFillPercent", true, ComparisonType.Equals)]
    [SerializeField] float spawnFillPercent = 1.0f;
    [DrawIf("hasSpawnFillPercent", false, ComparisonType.Equals)]
    [SerializeField] int spawnNumber = 1;

    #region Getter Setter
    public int SpawnID { get { return spawnID; } }
    public int SpawnNumber { get { return spawnNumber; } }
    public bool IsNetCodeObject { get { return isNetCodeObject; } }
    public Vector2Int Size { get { return size; } }
    public float SpawnRateChunck { get { return spawnRateChunck; } }
    public float SpawnFillPercent { get { return spawnFillPercent; } }
    public bool HasSpawnFillPercent { get { return hasSpawnFillPercent; } }
    public List<GameObject> Prefab { get { return prefab; } }
    public RotateMode RotateMode { get { return rotateMode; } }
    public float AngleMin { get { return angleMin; } }
    public float Angle { get { return angle; } }
    public bool WallObject { get { return wallObject; } }
    
    #endregion
}
