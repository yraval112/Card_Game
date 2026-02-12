const mongoose = require("mongoose");

const PlayerSchema = new mongoose.Schema({
  socketId: String,
  name: {
    type: String,
    default: "Anonymous"
  },
  reconnectToken: String,
  lastSeen: Date
});

module.exports = mongoose.model("Player", PlayerSchema);
