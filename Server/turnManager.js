const Room = require("./models/Room");
const { startRevealPhase } = require("./revealManager");
const { drawCard, getTurnCost } = require("./deckManager");

function handleTurnEvents(io) {
  // Note: This function is called from Index.js with the socket object
}

async function handleEndTurn(data, io, socket) {
  const { roomId, playerId, foldedCardIds } = data;

  try {
    const room = await Room.findOne({ roomId });
    if (!room) {
      socket.emit("message", { action: "error", message: "Room not found" });
      return;
    }

    // Store folded cards for this player
    room.gameState.foldedCards[playerId] = foldedCardIds;
    room.gameState.endedTurn[playerId] = true;

    // Clear hand after folding (cards are now played)
    room.playerDecks[playerId].hand = room.playerDecks[playerId].hand.filter(
      card => !foldedCardIds.includes(card.id)
    );

    await room.save();

    // Notify opponent of board state
    io.to(roomId).emit("message", {
      action: "syncBoard",
      opponentCardCount: foldedCardIds.length
    });

    // Check if both players ended turn
    if (room.gameState.endedTurn[room.players[0]] && room.gameState.endedTurn[room.players[1]]) {
      // Both players ready - start reveal phase
      io.to(roomId).emit("message", {
        action: "allPlayersReady"
      });

      startRevealPhase(roomId, room, io);
    }
  } catch (err) {
    console.error("Error handling end turn:", err);
    socket.emit("message", { action: "error", message: "Failed to end turn" });
  }
}

async function onTurnStart(roomId, io) {
  try {
    const room = await Room.findOne({ roomId });
    if (!room) return;

    // Draw +1 card for each player at turn start
    const [p1, p2] = room.players;
    
    const card1 = drawCard(room.playerDecks[p1]);
    const card2 = drawCard(room.playerDecks[p2]);

    // Reset turn state
    room.gameState.endedTurn[p1] = false;
    room.gameState.endedTurn[p2] = false;
    room.gameState.foldedCards[p1] = [];
    room.gameState.foldedCards[p2] = [];

    await room.save();

    const turnCost = getTurnCost(room.turn);

    // Send updated hand to each player
    io.to(roomId).emit("message", {
      action: "turnStart",
      turn: room.turn,
      turnCost: turnCost,
      drawnCard: { p1: card1, p2: card2 } // Each player only sees their own
    });

    // Send each player their updated hand
    io.sockets.sockets.forEach(sock => {
      if (sock.rooms && sock.rooms.has(roomId)) {
        const playerIndex = room.players.indexOf(sock.id);
        if (playerIndex >= 0) {
          sock.emit("message", {
            action: "syncHand",
            hand: room.playerDecks[room.players[playerIndex]].hand,
            turnCost: turnCost,
            turn: room.turn
          });
        }
      }
    });
  } catch (err) {
    console.error("Error on turn start:", err);
  }
}

module.exports = { handleTurnEvents, handleEndTurn, onTurnStart };
