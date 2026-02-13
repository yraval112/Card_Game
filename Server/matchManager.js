const Room = require("./models/Room");
const { initializeDecks, getTurnCost } = require("./deckManager");

let waitingPlayer = null;

function generateRoomId() {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
  let roomId = '';
  for (let i = 0; i < 6; i++) {
    roomId += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return roomId;
}

function createMatch(socket, io) {
  if (socket.rooms.size > 1) { 
    socket.emit("message", { 
      action: "error", 
      message: "You are already in a room!" 
    });
    return;
  }

  if (!waitingPlayer) {
    waitingPlayer = socket;
    const playerName = socket.playerName || "Anonymous";
    console.log(`Player ${socket.id} (${playerName}) is waiting for opponent...`);
    socket.emit("message", { 
      action: "waiting",
      message: "Searching for opponent..." 
    });
    return;
  }

  const roomId = generateRoomId();
  const player1Id = waitingPlayer.id;
  const player2Id = socket.id;
  const player1Name = waitingPlayer.playerName || "Anonymous";
  const player2Name = socket.playerName || "Anonymous";

  waitingPlayer.join(roomId);
  socket.join(roomId);

  console.log(`Match created: ${roomId} - ${player1Name} (${player1Id}) vs ${player2Name} (${player2Id})`);

  const playerDecks = initializeDecks(player1Id, player2Id);

  const newRoom = new Room({
    roomId,
    players: [player1Id, player2Id],
    playerNames: {
      [player1Id]: player1Name,
      [player2Id]: player2Name
    },
    turn: 1,
    maxPlayers: 2,
    gameState: {
      scores: { [player1Id]: 0, [player2Id]: 0 },
      foldedCards: { [player1Id]: [], [player2Id]: [] },
      endedTurn: { [player1Id]: false, [player2Id]: false },
      initiativePlayer: player1Id
    },
    playerDecks: playerDecks,
    status: "playing"
  });

  newRoom.save().then(() => {
    console.log(`Room ${roomId} saved to MongoDB (2/2 players)`);
  }).catch(err => console.error("Error saving room:", err));

  io.to(roomId).emit("message", {
    action: "gameStart",
    roomId,
    playerIds: [player1Id, player2Id],
    playerNames: {
      [player1Id]: player1Name,
      [player2Id]: player2Name
    },
    totalTurns: 6
  });

  waitingPlayer.emit("message", {
    action: "syncHand",
    hand: playerDecks[player1Id].hand,
    turnCost: getTurnCost(1),
    turn: 1
  });

  socket.emit("message", {
    action: "syncHand",
    hand: playerDecks[player2Id].hand,
    turnCost: getTurnCost(1),
    turn: 1
  });

  waitingPlayer = null;
}

function handleMatchmaking(io, socket) {
  socket.on("findMatch", () => {
    console.log(`Player ${socket.id} searching for match...`);
    createMatch(socket, io);
  });
}

module.exports = { handleMatchmaking, createMatch };
