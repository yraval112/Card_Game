using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string PlayerId;
    public string RoomId;

    public PlayerState LocalPlayer = new();
    public PlayerState OpponentPlayer = new();

    void Awake()
    {
        Instance = this;
    }

    public void OnGameStart(JObject msg)
    {
        RoomId = msg["roomId"].ToString();
        PlayerId = GameSocketManager.Instance.socket.Id;

        // Create local player
        LocalPlayer.playerName = GameSocketManager.Instance.playerName;
        LocalPlayer.Score = 0;

        // Send player data to server
        GameSocketManager.Instance.Send("enterRoom", new
        {
            roomId = RoomId,
            playerId = PlayerId,
            playerName = LocalPlayer.playerName,
            score = LocalPlayer.Score

        });

        Debug.Log("Game Started | Room: " + RoomId + " | Player: " + LocalPlayer.playerName);
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


    public void OnSyncBoard(JObject msg)
    {
        int count = msg["opponentCardCount"].Value<int>();
        Debug.Log("Opponent folded cards: " + count);
    }

    public void EndTurn(List<CardInstance> folded)
    {
        GameSocketManager.Instance.Send("endTurn", new
        {
            roomId = RoomId,
            playerId = PlayerId,
            foldedCards = folded.ConvertAll(c => c.data.id)
        });
    }

    public void OnAllPlayersReady()
    {
        Debug.Log("Reveal Phase Started");
    }

    public void OnRevealCard(JObject msg)
    {
        int cardId = msg["cardId"].Value<int>();
        string owner = msg["playerId"].ToString();

        Debug.Log($"Reveal Card {cardId} from {owner}");
    }

    public void OnTurnStart(JObject msg)
    {
        int turn = msg["turn"].Value<int>();
        Debug.Log("Turn Start: " + turn);
    }

    public void OnGameEnd()
    {
        Debug.Log("Game Ended");
    }
    public void OnReconnect(JObject msg)
    {
        Debug.Log("Reconnected - syncing state");

        JObject room = (JObject)msg["room"];
        // restore scores, turn, folded cards, etc.
    }
}
