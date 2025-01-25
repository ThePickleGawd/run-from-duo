// TODO: Handle all requests from user, which includes
// - Streaming voice input. We also need to know who we're talking to and some sort of game state
// - Streaming text respose back

import express from "express";
import dotenv from "dotenv";
// import { audioRouter } from "./routes/audioRoutes";
import http from "http";
import { Server } from "socket.io";

dotenv.config();

const app = express();
const server = http.createServer(app);
const io = new Server(server);

const PORT = process.env.PORT || 8000;

// Middleware
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Routes
// app.use("/api/audio", audioRouter);

// WebSocket for real-time audio streaming
io.on("connection", (socket) => {
  console.log("A user connected");

  socket.on("audio-stream", (data) => {
    // Handle incoming audio stream data
    console.log("Audio stream received:", data);
    // Broadcast the audio data to all connected clients
    socket.broadcast.emit("audio-stream", data);
  });

  socket.on("disconnect", () => {
    console.log("A user disconnected");
  });
});

server.listen(PORT, () => {
  console.log(`Server is running on http://localhost:${PORT}`);
});
