using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class HandUIManager : MonoBehaviour
{
    [SerializeField] private Transform handContainer;
    [SerializeField] private Transform playedCardsContainer;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private LayoutGroup layoutGroup;

    private Dictionary<CardInstance, CardUIElement> cardElements = new();

    void OnEnable()
    {
        EventBus.OnCardDrawn += AddCardToHand;
        EventBus.OnCardSelected += OnCardSelected;
        EventBus.OnCardDeselected += OnCardDeselected;
        EventBus.OnCardPlayed += OnCardPlayed;
        EventBus.OnCardUnplayed += OnCardUnplayed;
    }

    void OnDisable()
    {
        EventBus.OnCardDrawn -= AddCardToHand;
        EventBus.OnCardSelected -= OnCardSelected;
        EventBus.OnCardDeselected -= OnCardDeselected;
        EventBus.OnCardPlayed -= OnCardPlayed;
        EventBus.OnCardUnplayed -= OnCardUnplayed;
    }

    public void InitializeHand()
    {
        ClearHand();

        foreach (var card in DeckManager.Instance.Hand)
        {
            AddCardToHand(card);
        }
    }

    private void AddCardToHand(CardInstance card)
    {
        if (cardUIPrefab == null || handContainer == null)
        {
            Debug.LogError("Card UI prefab or hand container not assigned");
            return;
        }

        var cardUIObj = Instantiate(cardUIPrefab, handContainer);
        var cardUIComponent = cardUIObj.GetComponent<CardUIElement>();

        if (cardUIComponent != null)
        {
            cardUIComponent.Setup(card);
            cardElements[card] = cardUIComponent;
        }

        if (layoutGroup != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(handContainer as RectTransform);
    }

    private void OnCardSelected(CardInstance card)
    {
        if (cardElements.TryGetValue(card, out var element))
        {
            element.UpdateSelection(true);
        }
    }

    private void OnCardDeselected(CardInstance card)
    {
        if (cardElements.TryGetValue(card, out var element))
        {
            element.UpdateSelection(false);
        }
    }

    private void OnCardPlayed(CardInstance card)
    {
        if (cardElements.TryGetValue(card, out var element))
        {
            if (playedCardsContainer != null)
            {
                element.transform.SetParent(playedCardsContainer);
                element.UpdateSelection(false);
                Debug.Log($"Card moved to played area: {card.data.name}");
            }
        }

        if (layoutGroup != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(handContainer as RectTransform);

        if (playedCardsContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(playedCardsContainer as RectTransform);
    }

    private void OnCardUnplayed(CardInstance card)
    {
        if (cardElements.TryGetValue(card, out var element))
        {
            element.transform.SetParent(handContainer);
            element.UpdateSelection(false);
            Debug.Log($"Card returned to hand: {card.data.name}");
        }

        if (layoutGroup != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(handContainer as RectTransform);

        if (playedCardsContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(playedCardsContainer as RectTransform);
    }

    private void ClearHand()
    {
        foreach (Transform child in handContainer)
        {
            Destroy(child.gameObject);
        }

        cardElements.Clear();
    }
}
