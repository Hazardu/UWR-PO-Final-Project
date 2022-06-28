using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer : Player
{
	public AIPlayer(List<(int count, string cardname)> cards) : base(cards, new Vector3(0, 10, 0),  new Vector3(0, 10, 0))
	{
		var icon = InstantiateIcon(GameManager.instance.aiIconPrefab);
		icon.transform.position = Vector3.down * -3.93f;
	}


	public override void Die()
	{
		GameManager.instance.Win();
	}

	public override Player Enemy => GameManager.instance.players[0];

	public IEnumerator EnemyTurnCoroutine()
	{

		for (int i = 0; i < hand.Count; i++)
		{
			//Dumbly click all cards in your hand to play them on the table
			if (hand[i].Click())
				yield return new WaitForSeconds(1f);
		}

		for (int i = 0; i < playedCards.Count; i++)
		{
			var card = playedCards[i];
			GameManager.instance.SetCardAsAttacker(card);

			bool provocation = Enemy.playedCards.Any(x => x.provocation);
			if (card.roundsFrozen == 0)
			{
				foreach (var enemyCard in Enemy.playedCards)
				{
					if (provocation)
					{
						if (enemyCard.provocation)
						{
							GameManager.instance.SetVictim(enemyCard.unit);

							yield return new WaitForSeconds(0.5f);
							if (enemyCard.unit.GetAttacked())
								break;
						}
					}
					else
					{
						GameManager.instance.SetVictim(enemyCard.unit);
						yield return new WaitForSeconds(0.5f);
						if (enemyCard.unit.GetAttacked())
							break;
					}
				}
				if (card.roundsFrozen == 0)
				{
					GameManager.instance.SetVictim(Enemy.unit);
					yield return new WaitForSeconds(0.5f);
					Enemy.unit.GetAttacked();
				}
				GameManager.instance.ClearAttacker();
				GameManager.instance.SetVictim(null);

				yield return new WaitForSeconds(0.5f);

			}
		}
		yield return new WaitForSeconds(1.5f);

		GameManager.instance.EndTurn();
	}
}

