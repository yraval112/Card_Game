using UnityEngine;
using System;
using UnityEngine;
public class CardInstance
{
    public CardData data;

    public bool IsFolded { get; private set; }
    public int PlayOrder { get; private set; }

    public CardInstance(CardData cardData, int playOrder)
    {
        data = cardData;
        PlayOrder = playOrder;
    }

    public void Fold()
    {
        IsFolded = true;
    }

    public int GetBasePower()
    {
        return data.power;
    }
}


[Serializable]
public class CardData
{
    public int id;
    public string name;
    public int cost;
    public int power;
    public AbilityData ability;
}

[Serializable]
public class AbilityData
{
    public string type;   // GainPoints, StealPoints, DoublePower, etc.
    public int value;
}
