using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class WebSocketAudioClient : MonoBehaviour
{
    // Audio settings
    const int RATE = 24000;
    const int CHANNELS = 1;
    const int CHUNK = 1024; // frames per chunk

    ClientWebSocket websocket;
    AudioSource audioSource;

    // For live playback from received audio
    Queue<float> audioBuffer = new Queue<float>();
    object bufferLock = new object();

    // For microphone streaming
    bool isRecording = false;
    AudioClip micClip;
    int lastSamplePos = 0;
    CancellationTokenSource sendCts;

    private async void Start()
    {
        // Set up AudioSource with a streaming clip
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        AudioClip streamClip = AudioClip.Create("Stream", RATE * 10, CHANNELS, RATE, true, OnAudioRead);
        audioSource.clip = streamClip;
        audioSource.loop = true;
        audioSource.Play();

        await Connect();
    }


    public async Task ResetConnection()
    {
        await Disconnect();
        await Connect();
    }

    public async Task Connect()
    {
        // Connect to the WebSocket server
        websocket = new ClientWebSocket();
        Uri uri = new Uri($"ws://{GameManager.instance.baseURL}:8000");
        await websocket.ConnectAsync(uri, CancellationToken.None);
        Debug.Log("Connected to WebSocket server.");

        // Start the continuous receive loop (runs in the background)
        _ = Task.Run(() => ReceiveLoop());
    }

    public async Task Disconnect()
    {
        if (websocket == null || websocket.State != WebSocketState.Open)
        {
            Debug.Log("WebSocket is already closed or not initialized.");
            return;
        }

        Debug.Log("Disconnecting WebSocket...");

        try
        {
            await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);
            websocket.Dispose();
            websocket = null;
            Debug.Log("WebSocket disconnected.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while disconnecting WebSocket: {e.Message}");
        }
    }


    /// <summary>
    /// Call this when the user presses the button.
    /// </summary>
    public void StartSpeech()
    {
        if (isRecording) return;
        Debug.Log("StartSpeech: Starting microphone recording and streaming...");
        // Start recording – use a long duration with looping enabled
        micClip = Microphone.Start(null, true, 60, RATE);
        lastSamplePos = 0;
        isRecording = true;
        sendCts = new CancellationTokenSource();
        _ = Task.Run(() => StreamMicData(sendCts.Token));
    }

    /// <summary>
    /// Call this when the user releases the button.
    /// </summary>
    public async void StopSpeech()
    {
        if (!isRecording) return;
        Debug.Log("StopSpeech: Stopping microphone recording...");
        isRecording = false;
        sendCts.Cancel();
        Microphone.End(null);

        // Send any leftover samples
        int currentPos = Microphone.GetPosition(null);
        if (currentPos > lastSamplePos)
        {
            float[] samples = new float[currentPos - lastSamplePos];
            micClip.GetData(samples, lastSamplePos);
            byte[] pcmBytes = ConvertSamplesToPCM(samples);
            await websocket.SendAsync(new ArraySegment<byte>(pcmBytes),
                                      WebSocketMessageType.Binary,
                                      true,
                                      CancellationToken.None);
        }

        // Signal end of speech
        byte[] endMsg = Encoding.UTF8.GetBytes("END_OF_SPEECH");
        await websocket.SendAsync(new ArraySegment<byte>(endMsg),
                                  WebSocketMessageType.Text,
                                  true,
                                  CancellationToken.None);
        Debug.Log("Sent END_OF_SPEECH.");
    }

    /// <summary>
    /// Continuously polls the microphone for new data and sends it.
    /// </summary>
    async Task StreamMicData(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int pos = Microphone.GetPosition(null);
            // Handle wrap-around if needed (for very long recordings)
            if (pos < lastSamplePos)
                pos = micClip.samples;

            int newSamplesCount = pos - lastSamplePos;
            if (newSamplesCount > 0)
            {
                float[] samples = new float[newSamplesCount];
                micClip.GetData(samples, lastSamplePos);
                lastSamplePos = pos;

                byte[] pcmBytes = ConvertSamplesToPCM(samples);
                await websocket.SendAsync(new ArraySegment<byte>(pcmBytes),
                                          WebSocketMessageType.Binary,
                                          true,
                                          CancellationToken.None);
            }
            await Task.Delay(50); // adjust delay to balance latency and CPU usage
        }
    }

    byte[] ConvertSamplesToPCM(float[] samples)
    {
        byte[] pcmBytes = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short s = (short)(Mathf.Clamp(samples[i], -1f, 1f) * short.MaxValue);
            byte[] bytes = BitConverter.GetBytes(s);
            Buffer.BlockCopy(bytes, 0, pcmBytes, i * 2, 2);
        }
        return pcmBytes;
    }

    /// <summary>
    /// Continuously receives processed audio from the WebSocket and enqueues it.
    /// </summary>
    async Task ReceiveLoop()
    {
        byte[] buffer = new byte[CHUNK * 2];
        while (websocket.State == WebSocketState.Open)
        {
            var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (msg == "END_OF_OUTPUT")
                {
                    Debug.Log("Received END_OF_OUTPUT.");

                    // TODO: Don't allow speaking unti it's done
                    continue;
                }

                try
                {
                    var response = JsonConvert.DeserializeObject<FunctionCallResponse>(msg);

                    if (response.type == "function_call")
                    {
                        if (response.name == "reward_player")
                        {
                            // Deserialize the stringified JSON inside "arguments"
                            var argumentsJson = JsonConvert.DeserializeObject<FunctionArguments>(response.arguments);

                            Debug.Log($"Rewarding player with: {argumentsJson.reward}");

                            // Reward player on the Main Thread
                            StartCoroutine(GameManager.instance.RewardPlayer(argumentsJson.reward));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse JSON message: {ex.Message}");
                }

            }
            else if (result.MessageType == WebSocketMessageType.Binary)
            {
                int sampleCount = result.Count / 2;
                float[] floatSamples = new float[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    short s = BitConverter.ToInt16(buffer, i * 2);
                    floatSamples[i] = s / (float)short.MaxValue;
                }
                lock (bufferLock)
                {
                    foreach (var sample in floatSamples)
                        audioBuffer.Enqueue(sample);
                }
            }
        }
    }

    /// <summary>
    /// Called by the streaming AudioClip to fill audio data for playback.
    /// </summary>
    void OnAudioRead(float[] data)
    {
        lock (bufferLock)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = (audioBuffer.Count > 0) ? audioBuffer.Dequeue() : 0f;
        }
    }
}
