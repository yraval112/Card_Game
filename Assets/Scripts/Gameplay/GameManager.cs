using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string PlayerId;
    public string RoomId;

    public PlayerState LocalPlayer = new();
    public PlayerState OpponentPlayer = new();
    public LoginUI loginUI;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnGameStart(JObject msg)
    {
        Debug.Log("Game Started" + msg.ToString());
        Debug.Log($"loginUI is null? {loginUI == null}");
        EventBus.OnGameStart?.Invoke();
        // loginUI.gameObject.SetActive(false);
        loginUI.HideLoginUI();
        RoomId = msg["roomId"].ToString();
        PlayerId = GameSocketManager.Instance.socket.Id;

        // Create local player
        LocalPlayer.playerName = GameSocketManager.Instance.playerName;
        LocalPlayer.Score = 0;

        // Get opponent name from playerNames map
        var playerNames = msg["playerNames"];
        OpponentPlayer.playerName = playerNames[PlayerId == msg["playerIds"][0].ToString() ? msg["playerIds"][1].ToString() : msg["playerIds"][0].ToString()].ToString();

        Debug.Log($"Game Started | Room: {RoomId}");
        Debug.Log($"Local Player: {LocalPlayer.playerName}");
        Debug.Log($"Opponent: {OpponentPlayer.playerName}");

    }

    public void OnSetPlayerName(JObject msg)
    {
        string playerId = msg["playerId"].ToString();
        string playerName = msg["playerName"].ToString();

        if (playerId == PlayerId)
        {
            LocalPlayer.playerName = playerName;
            Debug.Log("Local player name set: " + playerName);
        }
        else
        {
            OpponentPlayer.playerName = playerName;
            Debug.Log("Opponent player name set: " + playerName);
        }
    }

    public void OnWaitingForOpponent(JObject msg)
    {
        Debug.Log(msg["message"].ToString());
        EventBus.OnWaitingForOpponent?.Invoke();

    }

    public void OnSyncHand(JObject msg)
    {
        // Parse hand from server
        var handArray = msg["hand"] as JArray;
        var turnCost = msg["turnCost"].Value<int>();
        var turn = msg["turn"].Value<int>();

        List<CardData> handData = new();
        foreach (var cardJson in handArray)
        {
            handData.Add(JsonUtility.FromJson<CardData>(cardJson.ToString()));
        }

        // Initialize deck manager
        DeckManager.Instance.InitializeHand(handData);
        DeckManager.Instance.SetTurnCost(turnCost);

        Debug.Log($"Hand synced: {handData.Count} cards, Turn Cost: {turnCost}");
    }

    public void OnSyncBoard(JObject msg)
    {
        int count = msg["opponentCardCount"].Value<int>();
        Debug.Log("Opponent folded cards: " + count);
        EventBus.OnOpponentCardCountChanged?.Invoke(count);
    }

    public void OnAllPlayersReady()
    {
        Debug.Log("Reveal Phase Started");
        EventBus.OnAllPlayersReady?.Invoke();
    }

    public void OnRevealCard(JObject msg)
    {
        int cardId = msg["cardId"].Value<int>();
        string owner = msg["playerId"].ToString();
        int orderIndex = msg["orderIndex"].Value<int>();

        Debug.Log($"Reveal Card {cardId} from {owner} at index {orderIndex}");

        // Update opponent's folded cards display
        if (owner != PlayerId)
        {
            // This is opponent's card
        }

        EventBus.OnRevealCard?.Invoke(new CardInstance(new CardData { id = cardId }, orderIndex));
    }

    public void OnScoreUpdated(JObject msg)
    {
        var scores = msg["scores"] as JObject;

        LocalPlayer.Score = scores[PlayerId].Value<int>();

        string opponentId = null;
        foreach (var key in scores.Properties().Select(p => p.Name))
        {
            if (key != PlayerId)
            {
                opponentId = key;
                break;
            }
        }

        if (opponentId != null)
        {
            OpponentPlayer.Score = scores[opponentId].Value<int>();
        }

        Debug.Log($"Score Updated - You: {LocalPlayer.Score}, Opponent: {OpponentPlayer.Score}");
        EventBus.OnScoreUpdated?.Invoke(LocalPlayer.Score, OpponentPlayer.Score);
    }

    public void OnTurnStart(JObject msg)
    {
        int turn = msg["turn"].Value<int>();
        int turnCost = msg["turnCost"].Value<int>();

        Debug.Log($"Turn Start: {turn}, Cost: {turnCost}");

        DeckManager.Instance.SetTurnCost(turnCost);
        EventBus.OnTurnStart?.Invoke(turn);
    }

    public void OnGameEnd(JObject msg)
    {
        var scores = msg["scores"] as JObject;

        LocalPlayer.Score = scores[PlayerId].Value<int>();

        string opponentId = null;
        foreach (var key in scores.Properties().Select(p => p.Name))
        {
            if (key != PlayerId)
            {
                opponentId = key;
                break;
            }
        }

        if (opponentId != null)
        {
            OpponentPlayer.Score = scores[opponentId].Value<int>();
        }

        Debug.Log($"Game Ended! You: {LocalPlayer.Score}, Opponent: {OpponentPlayer.Score}");
        EventBus.OnGameEnd?.Invoke();
    }

    public void OnReconnect(JObject msg)
    {
        Debug.Log("Reconnected - syncing state");
        JObject room = (JObject)msg["room"];

        // Restore game state
        if (room != null)
        {
            OnSyncFullState(room);
        }
    }

    public void OnSyncFullState(JObject room)
    {
        Debug.Log("Full state sync received");

        // This is called during reconnection to restore the entire game state
        // Would include: turn, scores, hand, board state, etc.
    }

    public void HandlePlayerDisconnected(JObject msg)
    {
        Debug.LogWarning("Opponent disconnected!");
        Debug.Log("Waiting for opponent to reconnect (10 seconds)...");

        string disconnectedPlayer = msg["disconnectedPlayer"].ToString();
        Debug.Log($"Disconnected player: {disconnectedPlayer}");

        // Trigger event for UI to show disconnected message
        EventBus.OnPlayerDisconnected?.Invoke();
    }

    public void OnOpponentQuit(JObject msg)
    {
        Debug.Log("Opponent quit the game!");

        string quitPlayer = msg["quitPlayer"].ToString();
        string message = msg["message"].ToString();
        Debug.Log($"Message: {message}");

        // Game is won by default
        LocalPlayer.Score += 1;

        // Trigger event for UI to show opponent quit message
        EventBus.OnOpponentQuit?.Invoke();
    }

    public void OnPlayerReconnected(JObject msg)
    {
        Debug.Log("Opponent has reconnected!");
        EventBus.OnPlayerReconnected?.Invoke();
    }
}

