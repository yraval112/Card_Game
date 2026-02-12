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
            case "gameStart":
                GameManager.Instance.OnGameStart(msg);
                break;

            case "syncBoard":
                GameManager.Instance.OnSyncBoard(msg);
                break;

            case "allPlayersReady":
                GameManager.Instance.OnAllPlayersReady();
                break;

            case "revealSingleCard":
                GameManager.Instance.OnRevealCard(msg);
                break;

            case "turnStart":
                GameManager.Instance.OnTurnStart(msg);
                break;

            case "gameEnd":
                GameManager.Instance.OnGameEnd();
                break;
            case "onReconnect":
                GameManager.Instance.OnReconnect(msg);
                break;
            case "setPlayerName":
                GameManager.Instance.OnSetPlayerName(msg);
                break;
        }
    }
}
