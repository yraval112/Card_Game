using System.Collections.Generic;

public class PlayerState
{
    public string playerName;
    public int Score;
    public List<CardInstance> Hand = new();
    public List<CardInstance> FoldedCards = new();
}
