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

  status: {
    type: String,
    enum: ["waiting", "playing", "finished", "full"],
    default: "waiting"
  }
});

module.exports = mongoose.model("Room", RoomSchema);
