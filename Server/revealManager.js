const Room = require("./models/Room");
const { getCardById } = require("./cardConfig");
const { resolveScore } = require("./scoreResolver");
const { onTurnStart } = require("./turnManager");


async function startRevealPhase(roomId, room, io) {
  const [p1, p2] = room.players;

  const s1 = room.gameState.scores[p1];
  const s2 = room.gameState.scores[p2];
  room.gameState.initiativePlayer =
    s1 === s2
      ? (Math.random() > 0.5 ? p1 : p2)
      : (s1 > s2 ? p1 : p2);

  await room.save();

  const queue = buildRevealQueue(room);
  revealNext(roomId, queue, io);
}

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

async function revealNext(roomId, queue, io) {
  const room = await Room.findOne({ roomId });
  if (!room) return;

  if (queue.length === 0) {
    await endTurn(roomId, room, io);
    return;
  }

  const { playerId, index } = queue.shift();
  const cardId = room.gameState.foldedCards[playerId][index];
  const cardData = getCardById(cardId);

  io.to(roomId).emit("message", {
    action: "revealSingleCard",
    playerId,
    cardId,
    orderIndex: index
  });

  resolveScore(cardData, playerId, room);

  await room.save();

  io.to(roomId).emit("message", {
    action: "scoreUpdated",
    scores: room.gameState.scores
  });

  setTimeout(() => {
    revealNext(roomId, queue, io);
  }, 1200);
}

async function endTurn(roomId, room, io) {
  room.turn++;

  room.players.forEach(p => {
    room.gameState.endedTurn[p] = false;
    room.gameState.foldedCards[p] = [];
  });

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

  await onTurnStart(roomId, io);
}

module.exports = { startRevealPhase };
