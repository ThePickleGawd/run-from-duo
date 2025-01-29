import WebSocket from "ws";
import { OpenAIRealtimeWS } from "openai/beta/realtime/ws";
import { PassThrough } from "stream";
import { config } from "./config/defaults";
import fs from "fs";

// Initialize WebSocket server
const wss = new WebSocket.Server({ port: config.port }, () => {
  console.log(`WebSocket server running on ws://localhost:${config.port}`);
});

wss.on("connection", (clientSocket) => {
  console.log("Client connected");
  const outputFileStream = fs.createWriteStream("output_audio.wav");

  // Initialize OpenAI WebSocket connection
  const openAISocket = new OpenAIRealtimeWS({
    model: "gpt-4o-realtime-preview-2024-12-17",
  });

  // Handle OpenAI WebSocket connection
  openAISocket.socket.on("open", () => {
    console.log("Connected to OpenAI WebSocket");

    // Update session for audio processing
    openAISocket.send({
      type: "session.update",
      session: {
        modalities: ["audio", "text"], // Use audio modality
        model: "gpt-4o-realtime-preview",
        instructions: "Respond to me in Chinese. Be brief.",
        input_audio_format: "pcm16",
        output_audio_format: "pcm16",
        turn_detection: undefined,
      },
    });
  });

  // Handle incoming audio from OpenAI
  openAISocket.on("response.audio.delta", (event) => {
    console.log("response.audio.delta - streaming to client");
    const processedChunk = Buffer.from(event.delta, "base64"); // Decode base64 audio
    clientSocket.send(processedChunk); // Send to client
    outputFileStream.write(processedChunk); // Save to file
  });

  openAISocket.on("input_audio_buffer.committed", (event) => {
    console.log("Committed!", event);
  });

  openAISocket.on("response.done", () => {
    console.log("response.done - saving audio and sending END_OF_OUTPUT");
    outputFileStream.end();
    openAISocket.send({ type: "input_audio_buffer.clear" });
    clientSocket.send("END_OF_OUTPUT");
  });

  // Handle client messages (audio input)
  clientSocket.on("message", (data) => {
    const msg = data.toString();

    if (msg === "END_OF_SPEECH") {
      console.log("END_OF_SPEECH Received");
      openAISocket.send({ type: "input_audio_buffer.commit" });
      openAISocket.send({ type: "input_audio_buffer.clear" });
      openAISocket.send({ type: "response.create" });
    } else {
      openAISocket.send({
        type: "input_audio_buffer.append",
        audio: data.toString("base64"), // Send audio as base64
      });
    }
  });

  // Handle client disconnection
  clientSocket.on("close", () => {
    console.log("Client disconnected");
    openAISocket.close(); // Close OpenAI WebSocket
  });

  // Handle errors
  clientSocket.on("error", (err) => console.error("Client socket error:", err));
  openAISocket.on("error", (err) => console.error("OpenAI socket error:", err));
});

console.log("WebSocket server listening for connections...");
