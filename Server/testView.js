require("dotenv").config();
const mongoose = require("./Db");
const Player = require("./models/Player");
const Room = require("./models/Room");

(async () => {
  try {
    const players = await Player.find();
    const rooms = await Room.find();
    
    console.log("\n========== MONGODB DATA ==========\n");
    
    console.log(`Total Players Connected: ${players.length}`);
    console.log("Players:");
    players.forEach((p, i) => {
      console.log(`  ${i + 1}. Name: ${p.name} | Socket ID: ${p.socketId}`);
    });
    
    console.log(`\nTotal Rooms Created: ${rooms.length}`);
    console.log("Rooms:");
    rooms.forEach((r, i) => {
      const playerList = r.players.map(pid => {
        const playerName = r.playerNames ? r.playerNames[pid] : "Unknown";
        return `${playerName}`;
      }).join(" vs ");
      
      console.log(`  ${i + 1}. Room ID: ${r.roomId}`);
      console.log(`     Players: ${playerList}`);
      console.log(`     Player Count: ${r.players.length}/${r.maxPlayers}`);
      console.log(`     Status: ${r.status}`);
      console.log(`     Turn: ${r.turn}`);
      console.log(`     Scores: ${JSON.stringify(r.gameState.scores)}`);
    });
    
    console.log("\n==================================\n");
  } catch (err) {
    console.error("Error:", err.message);
  }
  process.exit(0);
})();