const cards = require("./cards.json");

function getCardById(id) {
  return cards.find(c => c.id === id);
}

function getAllCards() {
  return cards;
}

module.exports = { getCardById, getAllCards };
