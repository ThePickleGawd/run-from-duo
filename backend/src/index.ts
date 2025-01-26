import WebSocket from 'ws';
import { OpenAIRealtimeWS } from 'openai/beta/realtime/ws';
import { PassThrough } from 'stream';

const PORT = 8080;

// Initialize WebSocket server
const wss = new WebSocket.Server({ port: PORT }, () => {
  console.log(`WebSocket server running on ws://localhost:${PORT}`);
});

wss.on('connection', (clientSocket) => {
  console.log('Client connected');

  // PassThrough stream to handle audio data from the client
  const inputAudioStream = new PassThrough();
  const outputAudioStream = new PassThrough();

  // Initialize OpenAI WebSocket connection
  const openAISocket = new OpenAIRealtimeWS({ model: 'gpt-4o-realtime-preview-2024-12-17' });

  // Handle OpenAI WebSocket connection
  openAISocket.socket.on('open', () => {
    console.log('Connected to OpenAI WebSocket');

    // Update session for audio processing
    openAISocket.send({
      type: 'session.update',
      session: {
        modalities: ['audio'], // Use audio modality
        model: 'gpt-4o-realtime-preview',
        instructions: "Do ______"
      },
    });

    // Pipe client audio to OpenAI
    inputAudioStream.on('data', (chunk) => {
      openAISocket.send({
        type: 'audio.chunk',
        chunk: chunk.toString('base64'), // Send audio as base64
      });
    });

    inputAudioStream.on('end', () => {
      openAISocket.send({ type: 'audio.end' }); // Signal end of audio stream
    });
  });

  // Handle incoming audio from OpenAI
  openAISocket.on('response.audio.chunk', (event) => {
    const processedChunk = Buffer.from(event.chunk, 'base64'); // Decode base64 audio
    outputAudioStream.write(processedChunk); // Write processed audio to output stream
  });

  openAISocket.on('response.done', () => {
    console.log('OpenAI audio processing completed');
    outputAudioStream.end(); // Signal end of processed audio
  });

  // Handle client messages (audio input)
  clientSocket.on('message', (data) => {
    inputAudioStream.write(data); // Pipe incoming client audio to inputAudioStream
  });

  // Pipe processed audio back to the client
  outputAudioStream.on('data', (chunk) => {
    clientSocket.send(chunk); // Send processed audio chunk to the client
  });

  outputAudioStream.on('end', () => {
    console.log('Finished streaming processed audio to client');
    clientSocket.close(); // Optionally close the client connection
  });

  // Handle client disconnection
  clientSocket.on('close', () => {
    console.log('Client disconnected');
    inputAudioStream.end(); // End input stream
    openAISocket.close(); // Close OpenAI WebSocket
  });

  // Handle errors
  clientSocket.on('error', (err) => console.error('Client socket error:', err));
  openAISocket.on('error', (err) => console.error('OpenAI socket error:', err));
});

console.log('WebSocket server listening for connections...');
