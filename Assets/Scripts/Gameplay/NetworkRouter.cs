using Newtonsoft.Json.Linq;
using UnityEngine;

public static class NetworkRouter
{
    public static void Handle(string json)
    {
        JObject msg = JObject.Parse(json);
        string action = msg["action"].ToString();
        Debug.Log("Received action: " + action);

        switch (action)
        {
            // Game State
            case "gameStart":
                Debug.Log("Routing to GameManager.OnGameStart");
                GameManager.Instance.OnGameStart(msg);
                break;

            case "turnStart":
                GameManager.Instance.OnTurnStart(msg);
                break;

            case "gameEnd":
                GameManager.Instance.OnGameEnd(msg);
                break;

            // Hand/Deck
            case "syncHand":
                GameManager.Instance.OnSyncHand(msg);
                break;

            case "syncBoard":
                GameManager.Instance.OnSyncBoard(msg);
                break;

            // Reveal Phase
            case "allPlayersReady":
                GameManager.Instance.OnAllPlayersReady();
                break;

            case "revealSingleCard":
                GameManager.Instance.OnRevealCard(msg);
                break;

            case "scoreUpdated":
                GameManager.Instance.OnScoreUpdated(msg);
                break;

            // Player Info
            case "playerNameSet":
                GameManager.Instance.OnSetPlayerName(msg);
                break;

            // Connection
            case "onReconnect":
                GameManager.Instance.OnReconnect(msg);
                break;

            case "syncFullState":
                GameManager.Instance.OnSyncFullState(msg);
                break;

            case "playerDisconnected":
                GameManager.Instance.HandlePlayerDisconnected(msg);
                break;

            case "opponentQuit":
                GameManager.Instance.OnOpponentQuit(msg);
                break;
            case "waiting":
                GameManager.Instance.OnWaitingForOpponent(msg);
                break;
            default:
                Debug.LogWarning($"Unknown action: {action}");
                break;
        }
    }
}
