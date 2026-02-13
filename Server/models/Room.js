const mongoose = require("mongoose");

const RoomSchema = new mongoose.Schema({
  roomId: String,
  players: [String],
  playerNames: {
    type: Map,
    of: String
  },
  turn: Number,
  maxPlayers: {
    type: Number,
    default: 2
  },

  gameState: {
    scores: Object,
    foldedCards: Object,
    endedTurn: Object,
    initiativePlayer: String
  },

  playerDecks: {
    type: Map,
    of: {
      deck: Array,      // Remaining cards in deck
      hand: Array,      // Current hand cards
      playedCards: Array // Cards played this turn
    }
  },

  status: {
    type: String,
    enum: ['waiting', 'ready', 'playing', 'completed', 'waiting_for_reconnection', 'finished'],
    default: 'waiting'
  },

  createdAt: {
    type: Date,
    default: Date.now
  }
});

module.exports = mongoose.model("Room", RoomSchema);
