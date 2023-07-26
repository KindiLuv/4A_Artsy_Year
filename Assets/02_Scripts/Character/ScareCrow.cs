public class ScareCrow : Enemy
{
    public override void TakeDamage(float damage)
    {        
        damageSameTime += damage;
    }
}