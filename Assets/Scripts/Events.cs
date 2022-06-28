public struct EventContext
{
	public Card target, attacker;
	public Player playerTarget;
	int RoundsAlive => target.roundsAlive;
	public Unit Unit => target ? target.unit : playerTarget.unit;
	public IHittable HitTarget => target ? target : playerTarget;

}

