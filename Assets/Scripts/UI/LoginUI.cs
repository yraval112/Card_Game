using TMPro;
using UnityEngine;
using DG.Tweening;

public class LoginUI : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public GameObject loginPanel;
    public GameObject mainPanel;
    public GameObject waitingText;
    public CanvasGroup loginCanvasGroup;

    void Awake()
    {
        // Subscribe to game start in Start instead of OnEnable
        // This ensures we stay subscribed even when panels are hidden
        Debug.Log("Subscribing to OnGameStart event in Awake");
        EventBus.OnGameStart += HideLoginUI;
    }

    void OnDestroy()
    {
        // Only unsubscribe when the component is destroyed
        EventBus.OnGameStart -= HideLoginUI;
    }

    public void OnSetBtnClick()
    {
        string playerName = usernameInput.text;
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Username cannot be empty");
            return;
        }

        GameSocketManager.Instance.Connect(playerName);
        loginPanel.SetActive(false);
        waitingText.SetActive(true);
    }

    public void HideLoginUI()
    {
        Debug.Log("Hiding login UI for both players");
        Debug.Log($"loginPanel ref: {loginPanel}, activeSelf: {loginPanel?.activeSelf}, activeInHierarchy: {loginPanel?.activeInHierarchy}");

        // Multiple approaches to ensure login panel is completely hidden
        if (loginCanvasGroup != null)
        {
            loginCanvasGroup.alpha = 0;
            loginCanvasGroup.interactable = false;
            loginCanvasGroup.blocksRaycasts = false;
        }

        // Disable the GameObject
        if (loginPanel != null)
        {
            loginPanel.SetActive(false);
        }

        // Hide waiting text
        if (waitingText != null)
        {
            waitingText.SetActive(false);
        }

        // Hide main panel
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        // Move login panel far away as extra safety
        if (loginPanel != null)
        {
            loginPanel.transform.position = new Vector3(-10000, -10000, 0);
        }

        Debug.Log("Login UI hidden completely");
    }

    public void OnDisconnectBtnClick()
    {
        Debug.Log("Disconnect button clicked");
        if (GameSocketManager.Instance != null)
        {
            GameSocketManager.Instance.Disconnect();
        }

        // Fully restore login UI
        if (loginCanvasGroup != null)
        {
            loginCanvasGroup.alpha = 1;
            loginCanvasGroup.interactable = true;
            loginCanvasGroup.blocksRaycasts = true;
        }

        if (loginPanel != null)
        {
            loginPanel.transform.position = Vector3.zero;
            loginPanel.SetActive(true);
        }

        waitingText.SetActive(false);
        mainPanel.SetActive(true);
        usernameInput.text = "";
    }
}
