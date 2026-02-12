using System;
using UnityEngine;

public static class EventBus
{
    public static Action OnGameStart;
    public static Action<int> OnTurnStart;
    public static Action<string> OnPlayerEndedTurn;
    public static Action OnAllPlayersReady;
    public static Action<CardInstance> OnRevealCard;
    public static Action<int, int> OnScoreUpdated;
    public static Action OnTurnEnd;
    public static Action OnGameEnd;
}
