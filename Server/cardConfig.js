const cards = require("./cards.json");

function getCardById(id) {
  return cards.find(c => c.id === id);
}

module.exports = { getCardById };
