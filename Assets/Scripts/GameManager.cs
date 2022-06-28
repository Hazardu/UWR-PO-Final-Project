
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TMPro;	//just to have a reference to UI labels
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

	public Player[] players;
	public CardDatabase cardDB;
	public GameObject cardPrefab;
	public AimLines aimLines;
	public int turn;

	[System.Serializable]
	public class DefaultDeckEntry
	{
		public int count;
		public string name;
	}
	[SerializeField] Transform[] TableSidePositions;
	[SerializeField] public List<DefaultDeckEntry> serializableDefaultDeck;
	[SerializeField] internal GameObject playerIconPrefab;
	[SerializeField] internal GameObject aiIconPrefab;
	[SerializeField] TextMeshProUGUI clockText, roundText, mpText, win, lose;

	public static GameManager instance;

	const float MAXTURNTIME = 60f;
	public float remainingTurnTime;

	int playerCurrentTurn;
	const int MAXPLAYERS = 2;

	public static bool HumanPlayerTurn => instance.playerCurrentTurn == 0; //set for PVE, true in PVP if it ever is added
	public static Player CurrentPlayer => instance.playerCurrentTurn != -1 ? instance.players[instance.playerCurrentTurn] : null;

	//Unity engine method, runts on first frame
	private IEnumerator Start()
	{
		instance = this;
		turn = 1;
		playerCurrentTurn = -1;
		cardDB = new CardDatabase();

		//init aim lines
		aimLines = new AimLines(FindObjectOfType<LineRenderer>());

		//set the countdown to 3 sec and wait
		remainingTurnTime = 3;
		InitializePlayers();
		yield return new WaitForSeconds(3);

		//begin the game
		FirstTurn();


	}

	private void Update()
	{
		remainingTurnTime -= Time.deltaTime; //time between frames
		clockText.text = string.Format("{0}:{1}", Mathf.Floor(remainingTurnTime / 60f), Mathf.Floor(remainingTurnTime % 60));

		//aim lines
		if (attacker != null)
		{
			if (HumanPlayerTurn)
			{
				if (!Input.GetMouseButton(0))
				{
					//released mouse button
					if (victim != null)
						victim.GetAttacked();
					ClearAttacker();
				}
				else
				{
					Vector3 pos = attacker.transform.position;
					Vector3 atk = victim != null ? victim.Center : Camera.main.ScreenToWorldPoint(Input.mousePosition);
					aimLines.UpdateAimLinePositions(pos, atk);
				}
			}
			else
			{
				Vector3 pos = attacker.transform.position;
				Vector3 atk = victim != null ? victim.Center : Vector3.zero;
				aimLines.UpdateAimLinePositions(pos, atk);
			}
		}


	}


	public void UpdateMpText()
	{
		if (players == null || players[0] == null)
			return;
		mpText.text = $"MP {players[0].actionPoints}/{players[0].maxActionPoints}";
	}


	public Card CreateCard(string name, Player p)
	{
		if (cardDB.cards.TryGetValue(name, out CardBase card))
		{
			var c = Instantiate(cardPrefab).GetComponent<Card>();
			c.SetUp(card, p);
			return c;
		}
		else
		{
			Debug.LogError("No card named " + name);
		}
		return null;
	}
	public Card CreateCard(CardBase cardb, Player p)
	{
		var c = Instantiate(cardPrefab).GetComponent<Card>();
		c.SetUp(cardb, p);
		return c;
	}


	private void FirstTurn()
	{
		remainingTurnTime = MAXTURNTIME;
		playerCurrentTurn = 0;
	}

	private void TurnBegin()
	{
		turn++;
		remainingTurnTime = MAXTURNTIME;
		playerCurrentTurn = 0;
		roundText.text = "Round " + turn;
	
	}


	//Method used by UI
	public void OnEndTurnButtonClick()
	{
		if (HumanPlayerTurn)
		{
			EndTurn();
		}
	}

	//End the current player's turn
	public void EndTurn()
	{
		ClearAttacker();
		{
			var p = CurrentPlayer;
			if (turn < 9)
				p.maxActionPoints++;
			p.actionPoints = p.maxActionPoints;
			for (int j = 0; j < p.playedCards.Count; j++)
			{
				p.playedCards[j].OnRoundEnd();
			}
			p.DrawCard();
		}

		remainingTurnTime = MAXTURNTIME;
		playerCurrentTurn++;
		if (playerCurrentTurn >= MAXPLAYERS)
		{
			TurnBegin();
		}
		else if (playerCurrentTurn > 0)
		{
			var AI = (AIPlayer)players[playerCurrentTurn];
			StartCoroutine(AI.EnemyTurnCoroutine());

		}
	}

	private void InitializePlayers()
	{
		players = new Player[MAXPLAYERS];
		var defaultDeckList = serializableDefaultDeck.Select(x => (x.count, x.name)).ToList();
		players[0] = new HumanPlayer(defaultDeckList);
		players[1] = new AIPlayer(defaultDeckList);
		for (int i = 0; i < MAXPLAYERS; i++)
		{
			int cardsToDraw = 4;
			if (i % 2 == 1)
			{
				cardsToDraw++;
			}
			players[i].DrawCards(cardsToDraw);
			players[i].tablePosition = TableSidePositions[i].position;
			players[i].SetStartingHealth();
		}

	}


	private Card attacker;
	private Unit victim;
	public void SetCardAsAttacker(Card card)
	{
		if (card.player == players[playerCurrentTurn] && card.Attack > 0)
		{
			victim = null;
			attacker = card;
			aimLines.EnableAimLine(card.unit.Center, Camera.main.ScreenToWorldPoint(Input.mousePosition));
		}
	}
	public void ClearAttacker()
	{
		victim = null;
		attacker = null;
		aimLines.DisableAimLine();
	}
	public void SetVictim(Unit u)
	{
		victim = u;
	}
	public void ClearVictim(Unit u)
	{
		if (victim == u)
			victim = null;
	}
	public Card Attacker => attacker;


	public void Win()
	{
		win.gameObject.SetActive(true);
		StartCoroutine(Restart());
	}
	public void Lose()
	{
		lose.gameObject.SetActive(true);
		StartCoroutine(Restart());
	}

	IEnumerator Restart()
	{
		remainingTurnTime = 10;
		playerCurrentTurn = -1;
		//Ten seconds to bask in glory of victory (there is no way you can lose!)
		yield return new WaitForSeconds(10f);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

	}
}
