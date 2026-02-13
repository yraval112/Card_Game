const { getCardById, getAllCards } = require("./cardConfig");

/**
 * Initialize 12-card deck for each player (shuffled)
 */
function initializeDecks(p1Id, p2Id) {
  const allCards = getAllCards();
  const p1Deck = shuffleDeck([...allCards]);
  const p2Deck = shuffleDeck([...allCards]);

  return {
    [p1Id]: {
      deck: p1Deck,
      hand: p1Deck.slice(0, 3), // Initial 3 cards
      playedCards: []
    },
    [p2Id]: {
      deck: p2Deck,
      hand: p2Deck.slice(0, 3), // Initial 3 cards
      playedCards: []
    }
  };
}

/**
 * Fisher-Yates shuffle algorithm
 */
function shuffleDeck(deck) {
  const shuffled = [...deck];
  for (let i = shuffled.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
  }
  return shuffled;
}

/**
 * Draw 1 card from deck for turn start
 */
function drawCard(playerDeck) {
  if (playerDeck.deck.length === 0) return null;
  const card = playerDeck.deck.shift();
  playerDeck.hand.push(card);
  return card;
}

/**
 * Get turn cost based on turn number (1-6)
 */
function getTurnCost(turnNumber) {
  return Math.min(turnNumber, 6);
}

module.exports = {
  initializeDecks,
  shuffleDeck,
  drawCard,
  getTurnCost
};
