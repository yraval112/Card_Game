using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [SerializeField] private Transform handContainer; // Parent transform for card prefabs
    [SerializeField] private GameObject cardPrefab; // Your card prefab
    [SerializeField] private int cardsToDistribute = 6;

    private List<CardInstance> localPlayerHand = new List<CardInstance>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Subscribe to events
        EventBus.OnAllPlayersReady += DistributeCards;
        EventBus.OnSyncHand += UpdateLocalPlayerHand;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        EventBus.OnAllPlayersReady -= DistributeCards;
        EventBus.OnSyncHand -= UpdateLocalPlayerHand;
    }

    public void DistributeCards()
    {
        Debug.Log($"Distributing {cardsToDistribute} cards to local player");

        // Clear existing cards from hand display
        ClearHandDisplay();

        // Get cards from local player's hand and display them
        if (localPlayerHand.Count > 0)
        {
            for (int i = 0; i < Mathf.Min(cardsToDistribute, localPlayerHand.Count); i++)
            {
                DisplayCard(localPlayerHand[i], i);
            }
        }
        else
        {
            Debug.LogWarning("No cards in local player hand to distribute");
        }
    }

    private void UpdateLocalPlayerHand(List<CardInstance> hand)
    {
        localPlayerHand = new List<CardInstance>(hand);
        Debug.Log($"Local player hand updated with {hand.Count} cards");
    }

    private void DisplayCard(CardInstance card, int index)
    {
        // Instantiate card prefab
        GameObject cardGO = Instantiate(cardPrefab, handContainer);
        cardGO.name = $"Card_{card.data.id}_{index}";

        // Get card UI component and set it up
        CardInstance cardUI = cardGO.GetComponent<CardInstance>();
        if (cardUI != null)
        {
            cardUI.SetCardData(card.data);
        }
        else
        {
            Debug.LogError("Card prefab missing CardInstance component");
        }
    }

    private void ClearHandDisplay()
    {
        foreach (Transform child in handContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public List<CardInstance> GetLocalPlayerHand()
    {
        return localPlayerHand;
    }

    public void AddCardToHand(CardInstance card)
    {
        localPlayerHand.Add(card);
        Debug.Log($"Card {card.data.id} added to local player hand. Total cards: {localPlayerHand.Count}");
    }

    public void RemoveCardFromHand(CardInstance card)
    {
        localPlayerHand.Remove(card);
        Debug.Log($"Card {card.data.id} removed from local player hand. Total cards: {localPlayerHand.Count}");
    }
}