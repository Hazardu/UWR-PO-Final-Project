using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public Player player;
	public int playerNumber;
	Vector3 handPos => player.m_offRoundCardPosition;
	// Start is called before the first frame update

	public float OffTurnScale = 0.3f;
	public float TurnScale = 0.9f;
	public float maxWidth = 14;

	private bool MyTurn => player.IsActiveTurn;
	public void UpdateHandPositions()
	{

		Vector3 pos;
		float scale;

		if (MyTurn)
		{
			pos = player.m_activeRoundCardPosition; //center of the screen
			scale = TurnScale;
		}
		else
		{
			pos = handPos;
			scale = OffTurnScale;
		}
		float spacing = MyTurn ? 1.1f : 1;
		spacing /= 1 + Mathf.Min(0, player.hand.Count - 5)*0.15f;
		float width = 0;
		foreach (var item in player.hand)
		{
			width += item.CardWidth * spacing;
		}
		if (width > maxWidth)
		{
			spacing /= 1 + (width - maxWidth) / maxWidth;
			width = maxWidth;
		}
		pos -= width * Vector3.right * 0.5f;
		int order = 0;
		foreach (var item in player.hand)
		{
			item.MoveTo(pos);
			item.CurrentScale = scale;
			item.SetOrderInLayer(order);
			order += 3;
			pos += item.CardWidth * spacing * Vector3.right;
		}





	}

	private void UpdateTablePosition()
	{
		float spacing = 1.14f;
		float width = 0;
		foreach (var item in player.playedCards)
		{
			width += item.CardWidth * spacing;
		}
		if (width > maxWidth)
		{
			spacing /= 1 + (width - maxWidth) / maxWidth;
			width = maxWidth;
		}
		int order = 0;
		var pos = player.tablePosition - Vector3.right * width*0.5f;
		foreach (var item in player.playedCards)
		{
			item.MoveTo(pos);
			pos += item.CardWidth * Vector3.right * spacing;
			item.CurrentScale = 0.5f;
			item.SetOrderInLayer(order);
			order += 3;

		}
	}

	private void Update()
	{
		//update position of cards 
		UpdateHandPositions();
		UpdateTablePosition();
	}

}
