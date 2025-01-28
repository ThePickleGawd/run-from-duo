import WebSocket from "ws";
import { OpenAIRealtimeWS } from "openai/beta/realtime/ws";
import { PassThrough } from "stream";
import { config } from "./config/defaults";

// Initialize WebSocket server
const wss = new WebSocket.Server({ port: config.port }, () => {
  console.log(`WebSocket server running on ws://localhost:${config.port}`);
});

wss.on("connection", (clientSocket) => {
  console.log("Client connected");

  // PassThrough stream to handle audio data from the client
  const inputAudioStream = new PassThrough();
  const outputAudioStream = new PassThrough();

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

    // Pipe client audio to OpenAI
    inputAudioStream.on("data", (chunk) => {
      openAISocket.send({
        type: "input_audio_buffer.append",
        audio: chunk.toString("base64"), // Send audio as base64
      });
    });
  });

  // Handle incoming audio from OpenAI
  openAISocket.on("response.audio.delta", (event) => {
    const processedChunk = Buffer.from(event.delta, "base64"); // Decode base64 audio
    outputAudioStream.write(processedChunk); // Write processed audio to output stream
  });

  openAISocket.on("response.done", () => {
    console.log("OpenAI audio processing completed");
    clientSocket.send("END_OF_OUTPUT");
  });

  // Handle client messages (audio input)
  clientSocket.on("message", (data) => {
    const msg = data.toString();

    if (msg === "END_OF_SPEECH") {
      console.log("END_OF_SPEECH Received");
      openAISocket.send({ type: "input_audio_buffer.commit" });
      openAISocket.send({ type: "input_audio_buffer.clear" });
    } else {
      console.log(data);
      inputAudioStream.write(data); // Pipe incoming client audio to inputAudioStream
    }
  });

  // Handle client disconnection
  clientSocket.on("close", () => {
    console.log("Client disconnected");
    inputAudioStream.end(); // End input stream
    openAISocket.close(); // Close OpenAI WebSocket
  });

  // Pipe processed audio back to the client
  outputAudioStream.on("data", (chunk) => {
    clientSocket.send(chunk); // Send processed audio chunk to the client
  });

  outputAudioStream.on("end", () => {
    console.log("Finished streaming processed audio to client");
  });

  // Handle errors
  clientSocket.on("error", (err) => console.error("Client socket error:", err));
  openAISocket.on("error", (err) => console.error("OpenAI socket error:", err));
});

console.log("WebSocket server listening for connections...");
