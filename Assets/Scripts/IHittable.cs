//Some interfaces shared between player and cards

//idea between attacker was to also include instant-cast spells like fireball from heartstone
public interface IAttacker
{
	public int Attack
	{
		get;set;
	}
}
public interface IHittable {
	public int Health
	{
		get; set;
	}
	public void TakeDamage(int dmg);
	public void Retaliate(IAttacker attacker);
	public void Die();
	public bool CanGetAttacked
	{get;
	}
}
