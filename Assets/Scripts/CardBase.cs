using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
[CreateAssetMenu(fileName = "card", menuName = "Card Definition", order = 0)]
public class CardBase : ScriptableObject
{
	[Flags]
	public enum CardTags
	{
		None = 0,
		Student = 1,
		Prof = 2,
		Provocation = 4,
		//etc
	}

	public int cost, health, attack;
	public string title, description;
	public CardTags tags;
	//public Color color;
	public Sprite icon;

	//unity's version of delegates
	[SerializeField] private UnityEvent<EventContext> onGetHit;
	[SerializeField] private UnityEvent<EventContext> onAttack;
	[SerializeField] private UnityEvent<EventContext> onRoundEnd;
	[SerializeField] private UnityEvent<EventContext> onCardDraw;
	[SerializeField] private UnityEvent<EventContext> onPlay, onDie;

	const int CONTEXTSHISTORYCOUNT = 2;
	public static EventContext[] contexts = new EventContext[CONTEXTSHISTORYCOUNT];
	private static int contextId = 0;
	public static EventContext context => contexts[contextId];
	private static void SetContext(in EventContext ctx)
	{
		contextId++;
		contextId = contextId % CONTEXTSHISTORYCOUNT;
		contexts[contextId] = ctx;
	}
	public void OnGetHit(in EventContext ctx)
	{
		SetContext(ctx);
		onGetHit?.Invoke(ctx);
	}
	public void OnAttack(in EventContext ctx)
	{
		SetContext(ctx);
		onAttack?.Invoke(ctx);
	}
	public void OnRoundEnd(in EventContext ctx)
	{
		SetContext(ctx);
		onRoundEnd?.Invoke(ctx);
	}
	public void OnCardDraw(in EventContext ctx)
	{
		SetContext(ctx);
		onCardDraw?.Invoke(ctx);
	}
	public void OnPlay(in EventContext ctx)
	{
		SetContext(ctx);
		onPlay?.Invoke(ctx);
	}
	public void OnDie(in EventContext ctx)
	{
		SetContext(ctx);
		onDie?.Invoke(ctx);
	}
	


	//card callbacks
	public void PlayCardForActivePlayer(string cardTitle)
	{
		GameManager.CurrentPlayer.PlayCardImmediate(GameManager.instance.cardDB.cards[cardTitle]);
	}

	public void Rush(EventContext ctx)
	{
		ctx.target.roundsFrozen = 0;
	}

	public void SummonByTag(int tag)
	{
		var students = GameManager.instance.cardDB.cards.Values.Where(x => (x.tags & (CardTags)tag) != 0).ToArray();
		GameManager.CurrentPlayer.PlayCardImmediate(students[UnityEngine.Random.Range(0, students.Count())]);
	}

	public void SleepTarget( int turns)
	{
		context.target.roundsFrozen += turns;
	}
	public void SleepAttacker( int turns)
	{
		context.attacker.roundsFrozen += turns;
	}
	public void AttackAllEnemies(int damage)
	{
		var e = GameManager.CurrentPlayer.Enemy;
		for (int i = 0; i < e.playedCards.Count; i++)
		{
			e.playedCards[i].TakeDamage(damage);
		}
	}
	public void AttackAllEnemiesAttackerDmg(EventContext ctx)
	{
		AttackAllEnemies(ctx.attacker.Attack);
	}
	public void AttackAllEnemiesTargetDmg(EventContext ctx)
	{
		AttackAllEnemies(ctx.target.Attack);
	}
	public void DestroyYourself(EventContext ctx)
	{
		ctx.target.Die();
	}

	public void ClearAllProvocationsOnEnemy(EventContext ctx)
	{
		var e = GameManager.CurrentPlayer.Enemy;
		for (int i = 0; i < e.playedCards.Count; i++)
		{
			e.playedCards[i].provocation = false;
		}
	}


	public void HealOtherCards(int healAmount)
	{
		var e = GameManager.CurrentPlayer;
		for (int i = 0; i < e.playedCards.Count; i++)
		{
			e.playedCards[i].Health += healAmount;
		}
	}
	public void IncreaseAttackOfAllies(int attackAmount)
	{
		var cards = GameManager.CurrentPlayer.playedCards;
		for (int i = 0; i < cards.Count; i++)
		{
			cards[i].Attack += attackAmount;
		}
	}
	public void ResurrectTargetsLast()
	{
		var grave = GameManager.CurrentPlayer.graveyard;
		if (grave.Count == 0)
			return;
		var lastDied = grave.First();
		if (lastDied != null)
		{
			grave.RemoveAt(0);
			lastDied.gameObject.SetActive(true);
			lastDied.provocation = (lastDied.cardbase.tags & CardBase.CardTags.Provocation) == CardBase.CardTags.Provocation;
			lastDied.player = GameManager.CurrentPlayer;
			lastDied.Health = lastDied.cardbase.health;
			GameManager.CurrentPlayer.PlayCard(lastDied);
		}

	}
	public void ResurrectLastEnemy()
	{
		var grave = GameManager.CurrentPlayer.Enemy.graveyard;
		if (grave.Count == 0)
			return;
		var lastDied = grave.First();
		if (lastDied != null)
		{
			grave.RemoveAt(0);
			lastDied.gameObject.SetActive(true);
			lastDied.provocation = (lastDied.cardbase.tags & CardBase.CardTags.Provocation) == CardBase.CardTags.Provocation;
			lastDied.player = GameManager.CurrentPlayer;
			lastDied.Health = lastDied.cardbase.health;
			GameManager.CurrentPlayer.PlayCard(lastDied);
		}

	}
	public void HitEnemyPlayer(int dmg = 10)
	{
		GameManager.CurrentPlayer.Enemy.TakeDamage(dmg);
	}
	public void DrawCard()
	{
		GameManager.CurrentPlayer.DrawCard();
	}
	public void AddMP()
	{
		GameManager.CurrentPlayer.maxActionPoints++;
		GameManager.CurrentPlayer.actionPoints++;
	}

	public void HealPlayer(int amount)
	{
		GameManager.CurrentPlayer.Health += amount;
	}

	public void MultiplyCardDamage(float mult)
	{
		var cards = GameManager.CurrentPlayer.playedCards;
		for (int i = 0; i < cards.Count; i++)
		{
			cards[i].Attack = Mathf.RoundToInt(cards[i].Attack * mult);
		}
	}
}

