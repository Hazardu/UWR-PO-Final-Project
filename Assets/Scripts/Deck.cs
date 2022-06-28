using System.Collections.Generic;

public class Deck
{
	public Stack<Card> stack;
	public Player player;
	private System.Random rng;

	public Deck(List<(int count, CardBase cardb)> cardBases, Player player) //param is a list of tuples
	{
		rng = new System.Random();
		this.player = player;
		stack = new Stack<Card>();
		Shuffle(CreateCardInstances(cardBases));
	}

	private List<Card> CreateCardInstances(in List<(int count, CardBase cardb)> cardBases)
	{
		var list = new List<Card>();
		for (int i = 0; i < cardBases.Count; i++)
		{
			for (int j = 0; j < cardBases[i].count; j++)
			{
				var card = GameManager.instance.CreateCard(cardBases[i].cardb,player);
				card.gameObject.SetActive(false);
				card.state = Card.CardState.Deck;
				list.Add(card);
			}
		}
		return list;
	}
	private void Shuffle(List<Card> cards)
	{

		stack.Clear();
		int n = cards.Count;
		while (n > 1)
		{
			int k = rng.Next(n--);
			Card temp = cards[n];
			cards[n] = cards[k];
			cards[k] = temp;
		}
		n = cards.Count;
		for (int i = 0; i < n; i++)
		{
			stack.Push(cards[i]);
		}
	}
}