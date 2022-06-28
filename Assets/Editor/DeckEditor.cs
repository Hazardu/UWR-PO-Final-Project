using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

[CustomEditor(typeof(GameManager))]
public class DeckEditor : Editor
{
	CardBase[] cards;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		GameManager gm = (GameManager)target;
		if (GUILayout.Button("Load cards from resources") || gm.serializableDefaultDeck == null || gm.serializableDefaultDeck.Count < cards.Length)
		{
			cards = CardDatabase.LoadCards();
			if (gm.serializableDefaultDeck == null)
				gm.serializableDefaultDeck = new List<GameManager.DefaultDeckEntry>();
			var names = gm.serializableDefaultDeck.Select(x => x.name).ToList();
			var list = new List<(int count, string name)>();
			foreach (var item in cards)
			{
				var idx = names.IndexOf(item.title);
				int count = idx != -1 ? gm.serializableDefaultDeck[idx].count : 1;

				gm.serializableDefaultDeck.Add(new GameManager.DefaultDeckEntry() { count = count, name = item.title });
			}
		}
		GUILayout.BeginVertical();
		for (int i = 0; i < cards.Length; i++)
		{
			GUILayout.BeginHorizontal();
			int inc = 0;
			if (GUILayout.Button("+"))
				inc = 1;
			if (GUILayout.Button("-"))
				inc = -1;
			GUILayout.Label(cards[i].title);
			GUILayout.Label(gm.serializableDefaultDeck[i].count.ToString());
			if (inc != 0)
				gm.serializableDefaultDeck[i].count = Mathf.Max(0, gm.serializableDefaultDeck[i].count + inc);
			GUILayout.EndHorizontal();
		}

		GUILayout.EndVertical();
	}

	public override VisualElement CreateInspectorGUI()
	{
		cards = CardDatabase.LoadCards();
		return base.CreateInspectorGUI();

	}
	// Start is called before the first frame update

}
