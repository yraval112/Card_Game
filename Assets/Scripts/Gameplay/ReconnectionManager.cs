using UnityEngine;
using Newtonsoft.Json.Linq;

public class ReconnectionManager : MonoBehaviour
{
    private string lastRoomId;
    private string lastPlayerId;
    private bool isReconnecting = false;

    void OnApplicationQuit()
    {
        // Save current room info for reconnection
        if (!string.IsNullOrEmpty(GameManager.Instance?.RoomId))
        {
            lastRoomId = GameManager.Instance.RoomId;
            lastPlayerId = GameManager.Instance.PlayerId;

            PlayerPrefs.SetString("LastRoomId", lastRoomId);
            PlayerPrefs.SetString("LastPlayerId", lastPlayerId);
            PlayerPrefs.Save();

            Debug.Log("Saved reconnection data - Room: " + lastRoomId);
        }
    }

    void OnApplicationFocus(bool focus)
    {
        if (focus && !isReconnecting && GameSocketManager.Instance?.socket != null)
        {
            // Check if we should reconnect
            TryReconnect();
        }
    }

    public void TryReconnect()
    {
        lastRoomId = PlayerPrefs.GetString("LastRoomId", "");
        lastPlayerId = PlayerPrefs.GetString("LastPlayerId", "");

        if (string.IsNullOrEmpty(lastRoomId) || string.IsNullOrEmpty(lastPlayerId))
        {
            Debug.Log("No previous game to reconnect to");
            return;
        }

        if (!GameSocketManager.Instance.socket.Connected)
        {
            Debug.Log("Socket not connected, cannot reconnect");
            return;
        }

        isReconnecting = true;
        Debug.Log($"Attempting to reconnect to room {lastRoomId}");

        GameSocketManager.Instance.Send("rejoinRoom", new
        {
            roomId = lastRoomId,
            oldPlayerId = lastPlayerId
        });
    }

    public void OnReconnectSuccess(JObject state)
    {
        isReconnecting = false;

        // Restore game state
        if (state != null)
        {
            var gameState = state["gameState"];
            var turn = state["turn"].Value<int>();
            var scores = state["gameState"]["scores"];

            // Restore scores
            GameManager.Instance.LocalPlayer.Score = scores[GameManager.Instance.PlayerId].Value<int>();

            // Restore hand
            // TODO: Get current hand from room state

            Debug.Log($"Reconnection successful - Turn {turn}, Your Score: {GameManager.Instance.LocalPlayer.Score}");
        }
    }

    public void OnDisconnected()
    {
        Debug.Log("Disconnected from server");
        isReconnecting = false;
    }
}
