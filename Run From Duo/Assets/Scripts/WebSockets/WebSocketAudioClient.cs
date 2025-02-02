using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketAudioClient : MonoBehaviour
{
    const int CHUNK = 1024;
    const int RATE = 24000;
    const int RECORD_SECONDS = 3;
    const int CHANNELS = 1;

    ClientWebSocket websocket;
    Queue<float> audioBuffer = new Queue<float>();
    object bufferLock = new object();

    AudioSource audioSource;

    async void Start()
    {
        // Set up an AudioSource with a streaming AudioClip.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Create a streaming clip that lasts 10 seconds (or longer) and use our PCM callback.
        AudioClip clip = AudioClip.Create("Stream", RATE * 10, CHANNELS, RATE, true, OnAudioRead);
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();

        // Connect to the WebSocket server.
        websocket = new ClientWebSocket();
        Uri uri = new Uri("ws://localhost:8000");
        await websocket.ConnectAsync(uri, CancellationToken.None);
        Debug.Log("Connected. Waiting 2 seconds for server setup...");
        await Task.Delay(2000);

        int sessionNumber = 1;
        while (websocket.State == WebSocketState.Open)
        {
            // Clear the buffer at the start of each session.
            lock (bufferLock) audioBuffer.Clear();
            await Session(sessionNumber);
            sessionNumber++;
        }
    }

    async Task Session(int sessionNumber)
    {
        Debug.Log($"Starting session {sessionNumber}");
        // Run recording/sending and receiving concurrently.
        Task sendTask = RecordAndSend();
        Task receiveTask = ReceiveAndPlay();
        await Task.WhenAll(sendTask, receiveTask);
        Debug.Log($"Session {sessionNumber} complete.");
    }

    async Task RecordAndSend()
    {
        Debug.Log($"Recording for {RECORD_SECONDS} seconds...");
        // Record from the default microphone.
        AudioClip micClip = Microphone.Start(null, false, RECORD_SECONDS, RATE);
        while (Microphone.GetPosition(null) <= 0) await Task.Delay(10);
        await Task.Delay(RECORD_SECONDS * 1000);
        Microphone.End(null);

        float[] samples = new float[micClip.samples * micClip.channels];
        micClip.GetData(samples, 0);

        // Convert float samples (-1..1) to 16-bit PCM.
        byte[] pcmBytes = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short s = (short)(Mathf.Clamp(samples[i], -1f, 1f) * short.MaxValue);
            byte[] bytes = BitConverter.GetBytes(s);
            Buffer.BlockCopy(bytes, 0, pcmBytes, i * 2, 2);
        }

        // Send PCM data in chunks.
        int chunkSize = CHUNK * 2;
        for (int i = 0; i < pcmBytes.Length; i += chunkSize)
        {
            int size = Math.Min(chunkSize, pcmBytes.Length - i);
            byte[] chunk = new byte[size];
            Array.Copy(pcmBytes, i, chunk, 0, size);
            await websocket.SendAsync(new ArraySegment<byte>(chunk), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        // Signal the end of the speech.
        byte[] endMessage = Encoding.UTF8.GetBytes("END_OF_SPEECH");
        await websocket.SendAsync(new ArraySegment<byte>(endMessage), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("Sent END_OF_SPEECH.");
    }

    async Task ReceiveAndPlay()
    {
        byte[] buffer = new byte[CHUNK * 2];
        while (true)
        {
            var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (message == "END_OF_OUTPUT")
                {
                    Debug.Log("Received END_OF_OUTPUT, ending session.");
                    break;
                }
            }
            else if (result.MessageType == WebSocketMessageType.Binary)
            {
                int sampleCount = result.Count / 2;
                float[] floatSamples = new float[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    short sample = BitConverter.ToInt16(buffer, i * 2);
                    floatSamples[i] = sample / (float)short.MaxValue;
                }
                lock (bufferLock)
                {
                    foreach (var s in floatSamples)
                        audioBuffer.Enqueue(s);
                }
            }
        }
    }

    // PCM callback that fills the AudioClip with data from our buffer.
    void OnAudioRead(float[] data)
    {
        lock (bufferLock)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = audioBuffer.Count > 0 ? audioBuffer.Dequeue() : 0f;
        }
    }
}
