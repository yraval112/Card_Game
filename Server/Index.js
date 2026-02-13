require("dotenv").config();
require("./Db.js");

const express = require("express");
const http = require("http");
const { Server } = require("socket.io");
const crypto = require("crypto");

const { handleMatchmaking } = require("./matchManager");
const { handleEndTurn, onTurnStart } = require("./turnManager");
const Room = require("./models/Room");
const Player = require("./models/Player");

const app = express();
const server = http.createServer(app);

const io = new Server(server, {
  cors: { origin: "*" }
});

app.get("/", (req, res) => {
  res.send("Card Game Server Running");
});

io.on("connection", (socket) => {
  console.log(`Player connected: ${socket.id}`);

  const reconnectToken = crypto.randomBytes(16).toString("hex");
  
  const newPlayer = new Player({
    socketId: socket.id,
    name: "Anonymous",
    reconnectToken,
    lastSeen: new Date()
  });
  newPlayer.save().catch(err => console.error("Error saving player:", err));

  socket.on("setPlayerName", async (playerName) => {
    try {
      const trimmedName = playerName.trim().substring(0, 20); 
      
      if (!trimmedName) {
        socket.emit("message", { 
          action: "error", 
          message: "Player name cannot be empty" 
        });
        return;
      }

      await Player.updateOne(
        { socketId: socket.id },
        { name: trimmedName }
      );

      socket.playerName = trimmedName;

      socket.emit("message", { 
        action: "playerNameSet", 
        name: trimmedName,
        message: `Welcome, ${trimmedName}!`
      });

      console.log(`Player ${socket.id} set name to: ${trimmedName}`);
    } catch (err) {
      console.error("Error setting player name:", err);
      socket.emit("message", { 
        action: "error", 
        message: "Failed to set player name" 
      });
    }
  });

  handleMatchmaking(io, socket);

  socket.on("endTurn", (data) => {
    handleEndTurn(data, io, socket);
  });

  socket.on("joinRoom", async ({ roomId }) => {
    try {
      const room = await Room.findOne({ roomId });
      
      if (!room) {
        socket.emit("message", { 
          action: "error", 
          message: "Room not found" 
        });
        return;
      }

      if (room.players.length >= room.maxPlayers) {
        room.status = "full";
        await room.save();
        socket.emit("message", { 
          action: "roomFull", 
          message: `Room ${roomId} is full (${room.players.length}/${room.maxPlayers})`,
          roomId,
          currentPlayers: room.players.length,
          maxPlayers: room.maxPlayers
        });
        console.log(`Player ${socket.id} tried to join full room ${roomId}`);
        return;
      }

      socket.emit("message", { 
        action: "error", 
        message: "Cannot join this room" 
      });
    } catch (err) {
      console.error("Error joining room:", err);
      socket.emit("message", { 
        action: "error", 
        message: "Failed to join room" 
      });
    }
  });

  socket.on("reconnectPlayer", async ({ roomId, playerId }) => {
    console.log(`Player ${playerId} reconnected to room ${roomId}`);
    const room = await Room.findOne({ roomId });
    if (!room) return;

    socket.join(roomId);

    socket.emit("message", {
      action: "syncFullState",
      room
    });
  });

  socket.on("rejoinRoom", async ({ roomId, oldPlayerId }) => {
    try {
      const room = await Room.findOne({ roomId });
      
      if (!room) {
        socket.emit("message", { 
          action: "error", 
          message: "Room not found" 
        });
        return;
      }

      if (!room.players.includes(oldPlayerId)) {
        socket.emit("message", { 
          action: "error", 
          message: "You are not part of this room" 
        });
        return;
      }

      room.players = room.players.map(p => p === oldPlayerId ? socket.id : p);
      room.status = "playing";
      await room.save();

      socket.join(roomId);

      io.to(roomId).emit("message", {
        action: "playerReconnected",
        newSocketId: socket.id,
        oldSocketId: oldPlayerId,
        message: `Player reconnected! Game resumed.`
      });

      socket.emit("message", {
        action: "syncFullState",
        room
      });

      console.log(`Player ${oldPlayerId} rejoined room ${roomId} with new socket ${socket.id}`);
    } catch (err) {
      console.error("Error during rejoin:", err);
      socket.emit("message", { 
        action: "error", 
        message: "Reconnection failed" 
      });
    }
  });

  socket.on("playerQuit", async () => {
    console.log(`Player ${socket.id} quit the game`);

    try {
      const room = await Room.findOne({ players: socket.id });
      
      if (room) {
        const otherPlayerId = room.players.find(p => p !== socket.id);
        
        room.status = "finished";
        
        if (otherPlayerId && room.status !== "finished") {
          room.gameState.scores[otherPlayerId] = 
            (room.gameState.scores[otherPlayerId] || 0) + 1;
        }
        
        await room.save();

        if (otherPlayerId) {
          io.to(room.roomId).emit("message", {
            action: "opponentQuit",
            quitPlayer: socket.id,
            message: "Opponent quit the game. You win by default!"
          });
        }

        console.log(`Room ${room.roomId} closed - Player ${socket.id} quit`);
      }

      socket.disconnect(true);
    } catch (err) {
      console.error("Error handling playerQuit:", err);
      socket.emit("message", { 
        action: "error", 
        message: "Failed to quit game" 
      });
    }
  });

  socket.on("disconnect", async () => {
    console.log(`Player disconnected: ${socket.id}`);

    try {
      const room = await Room.findOne({ players: socket.id });
      
      if (room) {
        if (room.status === "finished") {
          console.log(`Room ${room.roomId} - Game already finished. Cleaning up after player ${socket.id}.`);
          await Room.deleteOne({ _id: room._id });
          return;
        }

        const otherPlayerId = room.players.find(p => p !== socket.id);
        
        room.status = "waiting_for_reconnection";
        await room.save();

        if (otherPlayerId) {
          io.to(room.roomId).emit("message", {
            action: "playerDisconnected",
            disconnectedPlayer: socket.id,
            otherPlayer: otherPlayerId,
            message: "Opponent disconnected. Waiting for reconnection (10 seconds)..."
          });
        }

        console.log(`Room ${room.roomId} - Player ${socket.id} disconnected. Waiting for reconnection...`);

        setTimeout(async () => {
          const updatedRoom = await Room.findOne({ roomId: room.roomId });
          
          if (updatedRoom && updatedRoom.status === "waiting_for_reconnection") {
            updatedRoom.status = "finished";
            updatedRoom.gameState.scores[otherPlayerId] = 
              (updatedRoom.gameState.scores[otherPlayerId] || 0) + 1;
            await updatedRoom.save();

            io.to(room.roomId).emit("message", {
              action: "playerForfeit",
              message: `Opponent did not reconnect in time. ${otherPlayerId} wins by forfeit!`,
              winner: otherPlayerId
            });

            console.log(`Room ${room.roomId} - Player ${socket.id} forfeited. ${otherPlayerId} wins.`);
          }
        }, 10000);
      }

      await Player.updateOne(
        { socketId: socket.id },
        { lastSeen: new Date() }
      );

    } catch (err) {
      console.error("Error handling disconnect:", err);
    }
  });
});

server.listen(3000, () => {
  console.log("Server listening on port 3000");
});
