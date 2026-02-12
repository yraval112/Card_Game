const { rooms } = require("./matchManager");
const { startRevealPhase } = require("./revealManager");

function handleTurnEvents(socket, io) {

  socket.on("endTurn", (data) => {
    const { roomId, playerId, foldedCards } = data;
    const game = rooms[roomId];
    if (!game) return;

    game.players[playerId].foldedCards = foldedCards;
    game.players[playerId].endedTurn = true;

    socket.to(roomId).emit("message", {
      action: "syncBoard",
      opponentCardCount: foldedCards.length
    });

    const players = Object.values(game.players);
    if (players.every(p => p.endedTurn)) {
      io.to(roomId).emit("message", {
        action: "allPlayersReady"
      });

      startRevealPhase(roomId, game, io);
    }
  });
}

module.exports = { handleTurnEvents };
