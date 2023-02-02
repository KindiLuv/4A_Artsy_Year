using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/StaticEnemySO", order = 1)]
public class StaticEnemySO : ScriptableObject
{
    public string firstName;
    public int lives;
    public int speed;
    public Color color;
    public Vector3 goal1;
    public Vector3 goal2;
}
