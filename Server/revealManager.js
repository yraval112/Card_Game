const Room = require("./models/Room");
const { getCardById } = require("./cardConfig");
const { resolveScore } = require("./scoreResolver");

/**
 * ENTRY POINT
 * Called once both players have ended their turn
 */
async function startRevealPhase(roomId, room, io) {
  const [p1, p2] = room.players;

  const s1 = room.gameState.scores[p1];
  const s2 = room.gameState.scores[p2];

  // Initiative rule
  room.gameState.initiativePlayer =
    s1 === s2
      ? (Math.random() > 0.5 ? p1 : p2)
      : (s1 > s2 ? p1 : p2);

  await room.save();

  const queue = buildRevealQueue(room);
  revealNext(roomId, room, queue, io);
}

/**
 * Build alternating reveal order
 */
function buildRevealQueue(room) {
  const first = room.gameState.initiativePlayer;
  const second = room.players.find(p => p !== first);

  const firstCards = room.gameState.foldedCards[first];
  const secondCards = room.gameState.foldedCards[second];

  const max = Math.max(firstCards.length, secondCards.length);
  const queue = [];

  for (let i = 0; i < max; i++) {
    if (firstCards[i] !== undefined)
      queue.push({ playerId: first, index: i });

    if (secondCards[i] !== undefined)
      queue.push({ playerId: second, index: i });
  }

  return queue;
}

/**
 * Reveal cards one-by-one
 * Resolve score AFTER each reveal
 */
async function revealNext(roomId, room, queue, io) {

  // End reveal phase
  if (queue.length === 0) {
    await endTurn(roomId, room, io);
    return;
  }

  const { playerId, index } = queue.shift();
  const cardId = room.gameState.foldedCards[playerId][index];
  const cardData = getCardById(cardId);

  // 1️⃣ Reveal card to clients
  io.to(roomId).emit("message", {
    action: "revealSingleCard",
    playerId,
    cardId,
    orderIndex: index
  });

  // 2️⃣ Resolve score (SERVER ONLY)
  resolveScore(cardData, playerId, room);

  // 3️⃣ Save to MongoDB
  await room.save();

  // 4️⃣ Broadcast updated scores
  io.to(roomId).emit("message", {
    action: "scoreUpdated",
    scores: room.gameState.scores
  });

  // 5️⃣ Continue reveal sequence (delay for animation)
  setTimeout(() => {
    revealNext(roomId, room, queue, io);
  }, 1200);
}

/**
 * End turn or game
 */
async function endTurn(roomId, room, io) {
  room.turn++;

  // Reset turn state
  room.players.forEach(p => {
    room.gameState.endedTurn[p] = false;
    room.gameState.foldedCards[p] = [];
  });

  // Game end
  if (room.turn > 6) {
    room.status = "finished";
    await room.save();

    io.to(roomId).emit("message", {
      action: "gameEnd",
      scores: room.gameState.scores
    });
    return;
  }

  await room.save();

  io.to(roomId).emit("message", {
    action: "turnStart",
    turn: room.turn
  });
}

module.exports = { startRevealPhase };
