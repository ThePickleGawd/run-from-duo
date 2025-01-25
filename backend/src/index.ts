import WebSocket from 'ws';
import { PassThrough } from 'stream';
import { config } from './config/defaults';

// Simulated audio processor (replace this with actual processing logic)
function simulateAudioProcessing(inputStream: PassThrough, outputStream: PassThrough) {
  inputStream.on('data', (chunk) => {
    console.log(`Received audio chunk of size: ${chunk.length}`);
    // Simulate some delay in processing
    setTimeout(() => {
      // Echo back the same chunk (replace with real processing result)
      outputStream.write(chunk);
    }, 100); // Simulated processing delay (100ms)
  });

  inputStream.on('end', () => {
    console.log("Audio stream ended");
    outputStream.end();
  });
}

// Start the WebSocket server
const wss = new WebSocket.Server({ port: config.port }, () => {
  console.log('WebSocket server running on ws://localhost:8080');
});

wss.on('connection', (socket) => {
  console.log('Client connected');

  // Streams for audio data
  const inputAudioStream = new PassThrough();
  const outputAudioStream = new PassThrough();

  // Pipe the processed audio back to the WebSocket
  outputAudioStream.on('data', (chunk) => {
    socket.send(chunk);
  });

  socket.on('message', (data) => {
    // Write incoming audio data to the input stream
    inputAudioStream.write(data);
  });

  socket.on('close', () => {
    console.log('Client disconnected');
    inputAudioStream.end();
    outputAudioStream.end();
  });

  // Start audio processing
  simulateAudioProcessing(inputAudioStream, outputAudioStream);
});
