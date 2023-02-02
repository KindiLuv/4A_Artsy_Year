using UnityEngine;

public class IDamageable : MonoBehaviour
{
    private int hitPoints = 0;
    private bool _isAlive = false;

    public void TakeDamage(int damage)
    {
        if (damage > hitPoints)
        {
            damage = hitPoints;
        }
        hitPoints -= damage;
        if (hitPoints == 0)
        {
            _isAlive = false;
        }
    }

    public bool GetState()
    {
        return _isAlive;
    }

    public void AddLives(int lives)
    {
        hitPoints += lives;
        if (hitPoints > 0)
        {
            _isAlive = true;
        }
    }

    public int GetLives()
    {
        return hitPoints;
    }
}
