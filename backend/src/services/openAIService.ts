import WebSocket from "ws";
import { OpenAIRealtimeWS } from "openai/beta/realtime/ws";
import fs from "fs";
import wav from "wav";
import { SessionUpdateEvent } from "openai/resources/beta/realtime/realtime";

export const setupOpenAIWebSocket = (
  port: number,
  systemPrompt: string,
  openAITools: SessionUpdateEvent.Session.Tool[]
) => {
  const wss = new WebSocket.Server({ port: port }, () => {
    console.log(`WebSocket server running on ws://localhost:${port}`);
  });

  wss.on("connection", (clientSocket) => {
    console.log("Client connected");
    let turnCounter = 1;

    // Utility to create new file writers for each turn
    const createWavWriters = (turn: number) => {
      const inputWriter = new wav.FileWriter(
        `output/mic_input_turn_${turn}.wav`,
        {
          channels: 1,
          sampleRate: 24000,
          bitDepth: 16,
        }
      );
      const outputWriter = new wav.FileWriter(
        `output/ai_output_turn_${turn}.wav`,
        {
          channels: 1,
          sampleRate: 24000,
          bitDepth: 16,
        }
      );
      return { inputWriter, outputWriter };
    };

    let { inputWriter, outputWriter } = createWavWriters(turnCounter);

    // Set up the OpenAI connection
    const openAISocket = new OpenAIRealtimeWS({
      model: "gpt-4o-realtime-preview-2024-12-17",
    });

    openAISocket.socket.on("open", () => {
      console.log("Connected to OpenAI WebSocket");
      openAISocket.send({
        type: "session.update",
        session: {
          modalities: ["audio", "text"],
          model: "gpt-4o-realtime-preview",
          voice: "sage",
          instructions: systemPrompt,
          input_audio_format: "pcm16",
          output_audio_format: "pcm16",
          // @ts-ignore
          turn_detection: null, // This is correct (and the source of a lot of my previous frustration!)
          tools: openAITools,
          tool_choice: "auto",
        },
      });
    });

    // Stream audio delta responses back to the client and log them
    openAISocket.on("response.audio.delta", (event) => {
      console.log("response.audio.delta - streaming to client");
      const processedChunk = Buffer.from(event.delta, "base64");
      clientSocket.send(processedChunk);
      outputWriter.write(processedChunk);
    });

    // Log when input audio is committed
    openAISocket.on("input_audio_buffer.committed", (event) => {
      console.log("Input audio committed:", event);
    });

    // When the response is done, finish up the turn and prepare for the next one
    openAISocket.on("response.done", (event) => {
      console.log("response.done - finishing current turn");

      console.log(event.response.output);

      // Check for function call
      if (event.response.output) {
        for (const item of event.response.output) {
          if (item.type === "function_call") {
            const payload = JSON.stringify(item);
            clientSocket.send(payload); // Send payload to Unity VR
          }
        }
      }

      outputWriter.end();
      clientSocket.send("END_OF_OUTPUT");

      // Clear the input buffer and create new file writers for the next turn
      turnCounter++;
      ({ inputWriter, outputWriter } = createWavWriters(turnCounter));
    });

    // Handle incoming messages from the client
    clientSocket.on("message", (data) => {
      // If we get a string indicating end-of-turn, commit and trigger response generation.
      if (data.toString() === "END_OF_SPEECH") {
        console.log("END_OF_SPEECH received for turn", turnCounter);
        openAISocket.send({ type: "input_audio_buffer.commit" });
        openAISocket.send({ type: "response.create" });
        openAISocket.send({ type: "input_audio_buffer.clear" });
        inputWriter.end();
      } else if (Buffer.isBuffer(data)) {
        // Otherwise, treat it as audio data
        openAISocket.send({
          type: "input_audio_buffer.append",
          audio: data.toString("base64"),
        });
        inputWriter.write(data);
      } else {
        console.log("Received unrecognized message:", data);
      }
    });

    clientSocket.on("close", () => {
      console.log("Client disconnected");
      openAISocket.close();
    });

    clientSocket.on("error", (err) =>
      console.error("Client socket error:", err)
    );
    openAISocket.on("error", (err) =>
      console.error("OpenAI socket error:", err)
    );
  });

  console.log("WebSocket server listening for connections...");
};
