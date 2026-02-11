const express = require('express');
const http = require('http');
const socketIo = require('socket.io');
const mongoose = require('mongoose');
require('dotenv').config();

const app = express();
var server = http.createServer(app);
var io = socketIo(server);

// Middleware
app.use(express.json());

const DB = process.env.MONGO_URI;

mongoose.connect(DB).then(() => {
    console.log("Connected to MongoDB");
}).catch((err) => {
    console.error("Error connecting to MongoDB:", err);
});

const PORT = process.env.PORT || 3000;
server.listen(PORT, '0.0.0.0', () => {
    console.log(`Server is  running on port ${PORT}`);
});