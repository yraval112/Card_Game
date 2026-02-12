function createGameState(p1, p2) {
  return {
    scores: { [p1]: 0, [p2]: 0 },
    foldedCards: { [p1]: [], [p2]: [] },
    endedTurn: { [p1]: false, [p2]: false },
    initiativePlayer: null
  };
}

module.exports = { createGameState };
