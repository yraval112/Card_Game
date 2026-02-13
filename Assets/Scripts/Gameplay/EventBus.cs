using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    // Game Events
    public static Action OnGameStart;
    public static Action<int> OnTurnStart;
    public static Action<string> OnPlayerEndedTurn;
    public static Action OnAllPlayersReady;
    public static Action<CardInstance> OnRevealCard;
    public static Action<int, int> OnScoreUpdated;
    public static Action OnTurnEnd;
    public static Action OnGameEnd;

    // Deck/Hand Events
    public static Action<CardInstance> OnCardDrawn;
    public static Action<CardInstance> OnCardSelected;
    public static Action<CardInstance> OnCardDeselected;
    public static Action<CardInstance> OnCardPlayed;
    public static Action<CardInstance> OnCardUnplayed;
    public static Action<int> OnTurnCostUpdated;

    // Turn Timer Events
    public static Action<float> OnTurnTimerTick;
    public static Action OnTurnTimerExpired;

    // Board Events
    public static Action<int> OnOpponentCardCountChanged;

    // Disconnect Events
    public static Action OnPlayerDisconnected;
    public static Action OnOpponentQuit;
    // Waiting for Opponent
    public static Action OnWaitingForOpponent;
    public static Action OnPlayerReconnected;

    public static Action<List<CardInstance>> OnSyncHand;
}

