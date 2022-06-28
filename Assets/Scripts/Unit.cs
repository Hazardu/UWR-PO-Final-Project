using UnityEngine;

public class Unit : MonoBehaviour
{
	public IHittable connectedEntity;
	private BoxCollider2D col; //collider to detect if mouse is over the unit
	private void Start()
	{
		col = GetComponent<BoxCollider2D>();
	}

	public Vector3 Center => transform.position;
	private Player GetPlayer()
	{
		if (connectedEntity is Player p)
			return p;
		else if (connectedEntity is Card c)
			return c.player;
		return null;
	}

	private Card Attacker => GameManager.instance.Attacker;
	private void OnMouseEnter()
	{
		if (Attacker != null && this.GetPlayer() != Attacker.player && connectedEntity.CanGetAttacked)
		{
			GameManager.instance.SetVictim(this);
		}
	}

	private void OnMouseExit()
	{
		if (Attacker != null)
		{
			GameManager.instance.ClearVictim(this);
		}
	}
	public bool GetAttacked()
	{
		if (connectedEntity.CanGetAttacked)
		{

			if (connectedEntity is Player player)
			{
				if (player != Attacker.player)
				{
					player.TakeDamage(Attacker.Attack);
					Attacker.cardbase.OnAttack(new EventContext() { attacker = Attacker, playerTarget = player });
					Attacker.transform.position = Center;
					Attacker.roundsFrozen++;
					Attacker.sleepIcon.SetActive(true);

					return true;
				}
			}
			else if (connectedEntity is Card card)
			{

				if (card.player != Attacker.player)
				{
					connectedEntity.TakeDamage(Attacker.Attack);
					Attacker.cardbase.OnAttack(new EventContext() { attacker = Attacker, target = card });
					card.cardbase.OnGetHit(new EventContext() { attacker = Attacker, target = card });
					Attacker.transform.position = Center;
					connectedEntity.Retaliate(Attacker);
					Attacker.roundsFrozen++;
					Attacker.sleepIcon.SetActive(true);
					return true;

				}
			}
		}
		else
		{
			Debug.Log("Provocation prevents attacking");
		}
		return false;
	}

}

