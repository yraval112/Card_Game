const Room = require("./models/Room");
const { startRevealPhase } = require("./revealManager");
const { drawCard, getTurnCost } = require("./deckManager");

function handleTurnEvents(io) {
}

async function handleEndTurn(data, io, socket) {
  const { roomId, playerId, foldedCardIds } = data;

  try {
    const room = await Room.findOne({ roomId });
    if (!room) {
      socket.emit("message", { action: "error", message: "Room not found" });
      return;
    }

    room.gameState.foldedCards[playerId] = foldedCardIds;
    room.gameState.endedTurn[playerId] = true;

    room.playerDecks[playerId].hand = room.playerDecks[playerId].hand.filter(
      card => !foldedCardIds.includes(card.id)
    );

    await room.save();

    io.to(roomId).emit("message", {
      action: "syncBoard",
      opponentCardCount: foldedCardIds.length
    });

    if (room.gameState.endedTurn[room.players[0]] && room.gameState.endedTurn[room.players[1]]) {
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

    const [p1, p2] = room.players;
    
    const card1 = drawCard(room.playerDecks[p1]);
    const card2 = drawCard(room.playerDecks[p2]);

    room.gameState.endedTurn[p1] = false;
    room.gameState.endedTurn[p2] = false;
    room.gameState.foldedCards[p1] = [];
    room.gameState.foldedCards[p2] = [];

    await room.save();

    const turnCost = getTurnCost(room.turn);

    io.to(roomId).emit("message", {
      action: "turnStart",
      turn: room.turn,
      turnCost: turnCost,
      drawnCard: { p1: card1, p2: card2 } 
    });

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
