using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CardUIElement : MonoBehaviour
{
    public CardInstance CardInstance { get; private set; }

    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardPowerText;
    [SerializeField] private TextMeshProUGUI cardCostText;
    [SerializeField] private Image selectedOverlay;
    [SerializeField] private Button selectButton;

    public void Setup(CardInstance card)
    {
        CardInstance = card;

        if (cardNameText != null)
            cardNameText.text = card.data.name;

        if (cardPowerText != null)
            cardPowerText.text = $"POW {card.data.power}";

        if (cardCostText != null)
            cardCostText.text = $"Cost {card.data.cost}";

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnCardClicked);
        }

        UpdateSelection(false);
    }

    void OnCardClicked()
    {
        // Only allow selection from hand (not played cards)
        if (!DeckManager.Instance.Hand.Contains(CardInstance))
        {
            Debug.Log("Card already played");
            return;
        }

        // Toggle selection
        if (DeckManager.Instance.SelectedCard == CardInstance)
        {
            DeckManager.Instance.DeselectCard();
        }
        else
        {
            DeckManager.Instance.SelectCard(CardInstance);
        }
    }

    public void UpdateSelection(bool isSelected)
    {
        if (selectedOverlay != null)
            selectedOverlay.gameObject.SetActive(isSelected);
    }
}