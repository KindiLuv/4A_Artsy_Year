public class Enemy : Character
{
    public EnemySO enemyInformations;

    protected override void Awake()
    {
        base.Awake();
        team = Team.Enemy;
    }

    public void SetupEnemy()
    {        
        _maxHealth = enemyInformations.BaseHealth;
        _health = enemyInformations.BaseHealth;
        _prefab = enemyInformations.Prefab;
        _speed = enemyInformations.BaseSpeed;
    }
}
