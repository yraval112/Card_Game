function resolveScore(card, playerId, room) {
  let score = room.gameState.scores[playerId];
  const opponent = room.players.find(p => p !== playerId);

  // Base power
  score += card.power;

  // Ability handling
  if (card.ability) {
    switch (card.ability.type) {
      case "GainPoints":
        score += card.ability.value;
        break;

      case "StealPoints":
        room.gameState.scores[opponent] -= card.ability.value;
        score += card.ability.value;
        break;

      case "DoublePower":
        score += card.power; // add power again
        break;
    }
  }

  room.gameState.scores[playerId] = score;
}

module.exports = { resolveScore };
