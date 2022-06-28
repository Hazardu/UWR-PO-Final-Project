using System.Collections.Generic;
using UnityEngine;

public class CardDatabase
{
    public Dictionary<string, CardBase> cards;
    // Start is called before the first frame update
    public CardDatabase()
    {
        var cardsArr = Resources.LoadAll<CardBase>("Cards");

        cards = new Dictionary<string, CardBase>();
        foreach (var card in cardsArr)
        {
            cards.TryAdd(card.title, card);
        }
    }

    //for custom editor
    public static CardBase[] LoadCards()
    {
        return Resources.LoadAll<CardBase>("Cards");
     
    }
}

