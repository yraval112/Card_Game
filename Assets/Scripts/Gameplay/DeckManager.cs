using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    public List<CardInstance> Deck = new();
    public List<CardInstance> Hand = new();
    public List<CardInstance> PlayedCards = new();
    public CardInstance SelectedCard { get; private set; }

    public int AvailableCost { get; private set; }
    public int CurrentTurn { get; private set; } = 1;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Initialize hand from server data
    /// </summary>
    public void InitializeHand(List<CardData> handData)
    {
        Hand.Clear();
        for (int i = 0; i < handData.Count; i++)
        {
            Hand.Add(new CardInstance(handData[i], i));
        }

        Debug.Log($"Hand initialized with {Hand.Count} cards");
    }

    /// <summary>
    /// Draw a card (called at turn start)
    /// </summary>
    public void DrawCard(CardData cardData)
    {
        if (cardData == null) return;

        var cardInstance = new CardInstance(cardData, Hand.Count);
        Hand.Add(cardInstance);

        Debug.Log($"Drew card: {cardData.name}");
        EventBus.OnCardDrawn?.Invoke(cardInstance);
    }

    /// <summary>
    /// Select a card from hand (shows selection, doesn't play yet)
    /// </summary>
    public bool SelectCard(CardInstance card)
    {
        if (!Hand.Contains(card))
            return false;

        // Deselect previous card if any
        if (SelectedCard != null)
        {
            EventBus.OnCardDeselected?.Invoke(SelectedCard);
        }

        SelectedCard = card;
        EventBus.OnCardSelected?.Invoke(card);
        Debug.Log($"Card selected: {card.data.name}");
        return true;
    }

    /// <summary>
    /// Deselect current card
    /// </summary>
    public void DeselectCard()
    {
        if (SelectedCard != null)
        {
            EventBus.OnCardDeselected?.Invoke(SelectedCard);
            SelectedCard = null;
        }
    }

    /// <summary>
    /// Play the selected card (move to played area)
    /// </summary>
    public bool PlaySelectedCard()
    {
        if (SelectedCard == null)
        {
            Debug.LogWarning("No card selected!");
            return false;
        }

        // Check cost limit
        if (SelectedCard.data.cost + CalculatePlayedCardsCost() > AvailableCost)
        {
            Debug.LogWarning($"Cannot play card - cost limit reached ({CalculatePlayedCardsCost()}/{AvailableCost})");
            return false;
        }

        // Move card from hand to played
        PlayedCards.Add(SelectedCard);
        Hand.Remove(SelectedCard);
        SelectedCard.Fold();

        EventBus.OnCardPlayed?.Invoke(SelectedCard);
        Debug.Log($"Card played: {SelectedCard.data.name}");

        SelectedCard = null;
        return true;
    }

    /// <summary>
    /// Remove a card from played cards (undo)
    /// </summary>
    public void UnplayCard(CardInstance card)
    {
        if (PlayedCards.Contains(card))
        {
            PlayedCards.Remove(card);
            Hand.Add(card);
            EventBus.OnCardUnplayed?.Invoke(card);
            Debug.Log($"Card returned to hand: {card.data.name}");
        }
    }

    /// <summary>
    /// Calculate cost of all played cards this turn
    /// </summary>
    public int CalculatePlayedCardsCost()
    {
        return PlayedCards.Sum(c => c.data.cost);
    }

    /// <summary>
    /// Set available cost for this turn
    /// </summary>
    public void SetTurnCost(int cost)
    {
        AvailableCost = cost;
        CurrentTurn = cost;
        Debug.Log($"Turn {CurrentTurn} - Available Cost: {AvailableCost}");
        EventBus.OnTurnCostUpdated?.Invoke(AvailableCost);
    }

    /// <summary>
    /// Finalize played cards and send to server
    /// </summary>
    public void SubmitFold()
    {
        // Clear any selected card
        DeselectCard();

        var foldedCardIds = PlayedCards.Select(c => c.data.id).ToList();

        // Send to server
        GameSocketManager.Instance.Send("endTurn", new
        {
            roomId = GameManager.Instance.RoomId,
            playerId = GameManager.Instance.PlayerId,
            foldedCardIds = foldedCardIds
        });

        Debug.Log($"Folded {foldedCardIds.Count} cards");
        PlayedCards.Clear();
    }

    /// <summary>
    /// Clear all played cards (undo all)
    /// </summary>
    public void ClearAllPlayed()
    {
        foreach (var card in PlayedCards.ToList())
        {
            UnplayCard(card);
        }
    }

    public bool CanPlayMoreCards()
    {
        return CalculatePlayedCardsCost() < AvailableCost;
    }
}
