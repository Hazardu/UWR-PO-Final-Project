using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;

public class Player : IHittable
{
	public Unit unit;
	public Deck deck;
	public List<Card> graveyard;
	public List<Card> hand;
	public int _actionPoints, _maxActionPoints;
	public Vector3 tablePosition;
	public virtual Player Enemy { get; }
	public bool IsActiveTurn => this == GameManager.CurrentPlayer;
	public int actionPoints
	{
		get => _actionPoints;
		set
		{
			_actionPoints = value;
			OnActionPointChange();
		}
	}
	public int maxActionPoints
	{
		get => _maxActionPoints;
		set
		{
			_maxActionPoints = value;
			OnActionPointChange();
		}
	}

	protected int hp;

	public Vector3 m_offRoundCardPosition, m_activeRoundCardPosition;
	protected virtual void OnActionPointChange()
	{
	}

	public int Health
	{
		get => hp;
		set
		{
			Card.PlayAnimationOnStatChange(hp, value, hpAnimator);
			healthText.text = value.ToString();
			hp = value;
		}
	}

	public List<Card> playedCards;

	public Animator hpAnimator;
	public TextMeshPro healthText;

	public void OnCardDraw(Card card)
	{
		var ctx = new EventContext() { target = card };
		foreach (var item in playedCards)
		{
			item.cardbase.OnCardDraw(ctx);
		}
	}

	//Create new card and place it on the table
	public void PlayCardImmediate(CardBase cardBase)
	{
		PlayCard(GameManager.instance.CreateCard(cardBase, this));
	}

	public void PlayCard(Card card)
	{
		playedCards.Add(card);
		card.state = Card.CardState.Table;
		card.roundsFrozen = 1;
		card.roundsAlive = 1;
		card.cardbase.OnPlay(new EventContext() { target = card});
		card.sleepIcon.SetActive(card.roundsFrozen > 0);

	}


	protected GameObject InstantiateIcon(GameObject prefab)
	{
		var o = GameObject.Instantiate(prefab);
		hpAnimator = o.GetComponentInChildren<Animator>();
		healthText = o.GetComponentInChildren<TextMeshPro>();
		unit = o.GetComponent<Unit>();
		unit.connectedEntity = this;
		o.GetComponent<PlayerController>().player = this;
		return o;
	}
	protected Player(List<(int count, string cardname)> cards, Vector3 offRoundCardPosition, Vector3 activeRoundCardPosition)
	{
		m_offRoundCardPosition = offRoundCardPosition;
		m_activeRoundCardPosition = activeRoundCardPosition;

		maxActionPoints = 1;
		actionPoints = 1;

		int n = cards.Count;
		List<(int count, CardBase cardb)> cardBases = new List<(int count, CardBase cardb)>(n);
		for (int i = 0; i < n; i++)
		{
			var cardB = GameManager.instance.cardDB.cards[cards[i].cardname];
			cardBases.Add((cards[i].count, cardB));
		}
		deck = new Deck(cardBases, this);
		graveyard = new List<Card>();
		hand = new List<Card>();
		playedCards = new List<Card>();
	}
	public void SetStartingHealth()
	{
		hp = 40;
		healthText.text = Health.ToString();

	}

	public virtual void Die()
	{
	}
	
	public void Retaliate(IAttacker attacker)
	{
		//maybe add an event that triggers when you hit enemy player
	}
	public void TakeDamage(int dmg)
	{
		Health -= dmg;
		if (Health <= 0)
			Die();
	}

	public void DrawCards(int count)
	{
		for (int i = 0; i < count; i++)
		{
			DrawCard();
		}
	}
	public void DrawCard()
	{
		if (deck.stack.Count > 0)
		{
			var card = deck.stack.Pop();
			hand.Add(card);

			card.state = Card.CardState.Hand;
			card.gameObject.transform.position = Vector3.zero;
			card.MoveTo(m_offRoundCardPosition);
			card.gameObject.SetActive(true);
			card.sleepIcon.SetActive(false);

			OnCardDraw(card);
		}
		else
		{
			TakeDamage(8);
		}
	}

	public bool CanGetAttacked => !playedCards.Any(x => x.provocation);
}