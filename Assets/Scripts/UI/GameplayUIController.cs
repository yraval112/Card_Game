using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI localScoreText;
    [SerializeField] private TextMeshProUGUI opponentScoreText;
    [SerializeField] private TextMeshProUGUI localPlayerNameText;
    [SerializeField] private TextMeshProUGUI opponentPlayerNameText;
    [SerializeField] private TextMeshProUGUI opponentCardCountText;

    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button undoButton;
    [SerializeField] private Button playCardButton;

    private TurnTimer turnTimer;

    void Awake()
    {
        turnTimer = GetComponent<TurnTimer>();
        if (turnTimer == null)
        {
            turnTimer = gameObject.AddComponent<TurnTimer>();
        }
    }

    void OnEnable()
    {
        EventBus.OnTurnStart += UpdateTurnUI;
        EventBus.OnTurnCostUpdated += UpdateCostUI;
        EventBus.OnTurnTimerTick += UpdateTimerUI;
        EventBus.OnScoreUpdated += UpdateScoreUI;
        EventBus.OnOpponentCardCountChanged += UpdateOpponentCardCountUI;
        EventBus.OnCardPlayed += OnCardPlayed;
        EventBus.OnCardUnplayed += OnCardUnplayed;
        EventBus.OnGameStart += UpdatePlayerNames;

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnClicked);

        if (undoButton != null)
            undoButton.onClick.AddListener(OnUndoClicked);

        if (playCardButton != null)
            playCardButton.onClick.AddListener(OnPlayCardClicked);
    }

    void OnDisable()
    {
        EventBus.OnTurnStart -= UpdateTurnUI;
        EventBus.OnTurnCostUpdated -= UpdateCostUI;
        EventBus.OnTurnTimerTick -= UpdateTimerUI;
        EventBus.OnScoreUpdated -= UpdateScoreUI;
        EventBus.OnOpponentCardCountChanged -= UpdateOpponentCardCountUI;
        EventBus.OnCardPlayed -= OnCardPlayed;
        EventBus.OnCardUnplayed -= OnCardUnplayed;
        EventBus.OnGameStart -= UpdatePlayerNames;

        if (endTurnButton != null)
            endTurnButton.onClick.RemoveListener(OnEndTurnClicked);

        if (undoButton != null)
            undoButton.onClick.RemoveListener(OnUndoClicked);

        if (playCardButton != null)
            playCardButton.onClick.RemoveListener(OnPlayCardClicked);
    }

    private void UpdateTurnUI(int turn)
    {
        Debug.Log($"Updating turn UI: Turn {turn}");
        if (turnText != null)
            turnText.text = $"TURN {turn}/6";

        turnTimer.StartTurn();
    }

    private void UpdateCostUI(int cost)
    {
        Debug.Log($"Updating cost UI: Available Cost {cost}");
        if (costText != null)
        {
            int used = DeckManager.Instance.CalculatePlayedCardsCost();
            costText.text = $"Cost: {used}/{cost}";
        }
    }

    private void UpdateTimerUI(float timeRemaining)
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.Max(0, timeRemaining):F1}s";
    }

    private void UpdateScoreUI(int p1Score, int p2Score)
    {
        Debug.Log($"Updating score UI: Local {p1Score} - Opponent {p2Score}");
        if (localScoreText != null)
        {
            string playerName = GameManager.Instance.LocalPlayer.playerName;
            localScoreText.text = string.IsNullOrEmpty(playerName)
                ? $"Your Score: {GameManager.Instance.LocalPlayer.Score}"
                : $"{playerName}: {GameManager.Instance.LocalPlayer.Score}";
        }

        if (opponentScoreText != null)
        {
            string opponentName = GameManager.Instance.OpponentPlayer.playerName;
            opponentScoreText.text = string.IsNullOrEmpty(opponentName)
                ? $"Opponent: {GameManager.Instance.OpponentPlayer.Score}"
                : $"{opponentName}: {GameManager.Instance.OpponentPlayer.Score}";
        }

        UpdatePlayerNames();
    }

    private void UpdateOpponentCardCountUI(int count)
    {
        Debug.Log($"Updating opponent card count UI: Opponent has {count} cards left");
        if (opponentCardCountText != null)
            opponentCardCountText.text = $"Opponent Folded: {count} cards";
    }

    private void OnCardPlayed(CardInstance card)
    {
        UpdateCostUI(DeckManager.Instance.AvailableCost);

        // Disable end turn button if can't play more cards
        if (endTurnButton != null)
            endTurnButton.interactable = true;
    }

    private void OnCardUnplayed(CardInstance card)
    {
        UpdateCostUI(DeckManager.Instance.AvailableCost);
    }

    private void OnEndTurnClicked()
    {
        Debug.Log("End Turn clicked");
        DeckManager.Instance.SubmitFold();
        turnTimer.StopTurn();
    }

    private void OnUndoClicked()
    {
        Debug.Log("Undo last card");
        if (DeckManager.Instance.PlayedCards.Count > 0)
        {
            var card = DeckManager.Instance.PlayedCards[DeckManager.Instance.PlayedCards.Count - 1];
            DeckManager.Instance.UnplayCard(card);
        }
    }

    private void OnPlayCardClicked()
    {
        Debug.Log("Play Card clicked");
        bool success = DeckManager.Instance.PlaySelectedCard();

        if (!success && DeckManager.Instance.SelectedCard != null)
        {
            Debug.LogWarning("Cannot play card - cost limit reached!");
        }
    }

    private void UpdatePlayerNames()
    {
        if (localPlayerNameText != null)
        {
            string playerName = GameManager.Instance.LocalPlayer.playerName;
            localPlayerNameText.text = string.IsNullOrEmpty(playerName) ? "You" : playerName;
        }

        if (opponentPlayerNameText != null)
        {
            string opponentName = GameManager.Instance.OpponentPlayer.playerName;
            opponentPlayerNameText.text = string.IsNullOrEmpty(opponentName) ? "Opponent" : opponentName;
        }
    }
}
