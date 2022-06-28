using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class Card : MonoBehaviour, IHittable, IAttacker
{
	public enum CardState { Grave, Deck, Hand, Table };
	public CardState state;
	public int roundsFrozen;
	public int roundsAlive;
	public CardBase cardbase;
	public Unit unit;
	public Player player;
	public Player Enemy => player.Enemy;
	private bool _provocation;
	public bool provocation
	{
		get => _provocation; set
		{
			nameText.color = value ? Color.red : Color.white;
			_provocation = value;
		}
	}

	//displayed text elements
	public GameObject sleepIcon;
	private TextMeshPro healthText, costText, attackText, nameText, descText;
	private Animator hpAnim, costAnim, attackAnim;
	private SpriteRenderer icon;
	[SerializeField] private Renderer[] renderers;
	[SerializeField] private SpriteMask mask;

	//animation
	const string upgradeAnimationName = "Upgrade";
	const string downradeAnimationName = "Downgrade";
	const float cardWidth = 3.392969f;
	const float cardHeight = 5.130764f;

	// card width during current frame
	public float CardWidth => cardWidth * transform.localScale.x;
	public float CardHeight => cardHeight * transform.localScale.y;


	public void OnRoundEnd()
	{
		roundsFrozen--;
		roundsAlive++;
		if (roundsFrozen <= 0)
		{
			sleepIcon.SetActive(false);
			roundsFrozen = 0;
			cardbase.OnRoundEnd( new EventContext() { target=this } );
		}

	}

	//set card to be displayed above others if mouse is over it
	bool highlighted = false;
	public void SetOrderInLayer(int increase)
	{
		if (highlighted)
			increase = 1000;
		renderers[0].sortingOrder = 50 + increase;
		for (int i = 1; i < renderers.Length - 1; i++)
		{
			renderers[i].sortingOrder = 51 + increase;
		}
		renderers[renderers.Length - 1].sortingOrder = 52 + increase;
		mask.backSortingOrder = 50 + increase;
		mask.frontSortingOrder = 51 + increase;
	}
	private int hp, cost, atk;
	public static void PlayAnimationOnStatChange(int current, int newValue, Animator animator)
	{
		if (current > newValue)
			animator.Play(downradeAnimationName);
		else if (current < newValue)
			animator.Play(upgradeAnimationName);
	}
	public int Health
	{
		get => hp;
		set
		{
			PlayAnimationOnStatChange(hp, value, hpAnim);
			hp = value;
			healthText.text = hp.ToString();
		}
	}
	public int Cost
	{
		get => cost;
		set
		{
			PlayAnimationOnStatChange(cost, value, costAnim);
			cost = value;
			costText.text = cost.ToString();
		}
	}
	public int Attack
	{
		get => atk;
		set
		{
			PlayAnimationOnStatChange(atk, value, attackAnim);
			atk = value;
			attackText.text = atk.ToString();
		}
	}

	public void Retaliate(IAttacker attacker)
	{
		if (attacker is Card card)
		{
			//if(Health>0)
			card.TakeDamage(Attack);
		}
	}
	public void Die()
	{
		state = CardState.Grave;
		player.playedCards.Remove(this);
		cardbase.OnDie(new EventContext() { target = this });
		player.graveyard.Add(this);
		StartCoroutine(FlyOffScreenAndDie());
	}

	IEnumerator FlyOffScreenAndDie()
	{
		MoveTo(Vector3.right * 25);
		yield return new WaitForSeconds(2);
		gameObject.SetActive(false);
	}


	public void TakeDamage(int dmg)
	{
		Health -= dmg;
		if (Health <= 0)
			Die();
	}


	public void SetUp(CardBase cb, Player p)
	{
		cardbase = cb;
		//get text components
		costText = transform.GetChild(0).GetComponent<TextMeshPro>();
		attackText = transform.GetChild(1).GetComponent<TextMeshPro>();
		healthText = transform.GetChild(2).GetComponent<TextMeshPro>();

		if (!costText || !attackText || !healthText)
			Debug.LogError("One of the card text components was not found");

		icon = transform.GetChild(3).GetChild(0).GetComponent<SpriteRenderer>();
		nameText = transform.GetChild(4).GetComponent<TextMeshPro>();
		descText = transform.GetChild(5).GetComponent<TextMeshPro>();
		hpAnim = healthText.gameObject.GetComponent<Animator>();
		attackAnim = attackText.gameObject.GetComponent<Animator>();
		costAnim = costText.gameObject.GetComponent<Animator>();

		unit = GetComponent<Unit>();
		unit.connectedEntity = this;

		Health = cardbase.health;
		Attack = cardbase.attack;
		Cost = cardbase.cost;
		nameText.text = cardbase.title;
		descText.text = cardbase.description;
		provocation = (cardbase.tags & CardBase.CardTags.Provocation) == CardBase.CardTags.Provocation;
		icon.sprite = cardbase.icon;
		state = CardState.Deck;
		player = p;
	}

	private Vector3 positionTarget;
	const float SPEED = 10.0f;
	public void MoveTo(Vector3 pos)
	{
		positionTarget = pos;
	}

	//called once per frame
	void Update()
	{
		var pos = transform.position;
		transform.position = Vector3.Slerp(pos, positionTarget, (Vector3.SqrMagnitude(positionTarget - pos) + SPEED) * Time.deltaTime);
		var scale = transform.localScale.x;
		transform.localScale = Mathf.Lerp(scale, (scaleTarget + CurrentScale) / 1.4f, Time.deltaTime * SPEED) * Vector3.one;

	}

	const float ONHOVERSCALEBONUS = 0.25f;
	private float _currentScale = 1, scaleTarget;
	public float CurrentScale
	{
		get => _currentScale;
		set
		{
			_currentScale = value;

		}
	}


	public bool CanGetAttacked => player.playedCards.Any(x => x.provocation) ? provocation : true;


	void OnMouseEnter()
	{
		scaleTarget = ONHOVERSCALEBONUS;
		highlighted = true;
	}
	void OnMouseExit()
	{
		scaleTarget = 0;
		highlighted = false;
	}


	private void OnMouseDown()
	{
		if (player.IsActiveTurn)
			Click();
	}
	public bool Click()
	{
		switch (state)
		{
			//case CardState.Grave:
			//	break;
			//case CardState.Deck:
			//	break;
			case CardState.Hand:
				if (player.actionPoints >= Cost)
				{
					player.actionPoints -= Cost;
					player.hand.Remove(this);
					player.PlayCard(this);
					CurrentScale = 0.6f;
					return true;
				}
				break;
			case CardState.Table:
				if (roundsFrozen == 0)
				{
					GameManager.instance.SetCardAsAttacker(this);
					return true;

				}
				break;
			default:
				break;
		}
		return false;
	}
}
