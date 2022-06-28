using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player
{
	public HumanPlayer(List<(int count, string cardname)> cards) : base(cards, new Vector3(3.71f, -3.9974f, 0), Vector3.down * 3.4f + Vector3.right)
	{
		var o = InstantiateIcon(GameManager.instance.playerIconPrefab);
		o.transform.position = Vector3.down * 3.23f + Vector3.left * 6.6f;
	}
	protected override void OnActionPointChange() => GameManager.instance.UpdateMpText();
	public override Player Enemy => GameManager.instance.players[1];
	public override void Die()
	{
		GameManager.instance.Lose();
	}
}
