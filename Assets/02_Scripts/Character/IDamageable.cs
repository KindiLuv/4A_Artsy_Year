public interface IDamageable
{
    void TakeDamage(float damage);
    void HealDamage(float heal);
    void DropLoot();
    void Death();
    bool isAlive();
    Team GetTeam();
}
