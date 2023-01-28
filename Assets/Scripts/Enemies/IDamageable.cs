using UnityEngine;

public class IDamageable : MonoBehaviour
{
    private int _lives = 0;
    private bool _isAlive = false;

    public void TakeDamage(int damage)
    {
        if (damage > _lives)
        {
            damage = _lives;
        }
        _lives -= damage;
        if (_lives == 0)
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
        _lives += lives;
        if (_lives > 0)
        {
            _isAlive = true;
        }
    }

    public int GetLives()
    {
        return _lives;
    }
}
