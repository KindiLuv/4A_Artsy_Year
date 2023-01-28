using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/EnemySO", order = 1)]
public class EnemySO : ScriptableObject
{
    public string firstName;
    public int lives;
    public int speed;
    public WeaponSO weapon;
    public Color color;
}
