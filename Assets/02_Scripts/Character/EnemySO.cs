using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Characters/Enemy", order = 1)]
public class EnemySO : ScriptableObject
{
    public float BaseHealth;
    public float BaseSpeed;
    public float BaseDamage;
    public GameObject Prefab;
}
