import asyncio
import websockets
import pyaudio
import wave

HOST = "localhost"
PORT = 8000
OUTPUT_FILE = "output.wav"

# Audio config
CHUNK = 1024
RATE = 16000
CHANNELS = 1
FORMAT = pyaudio.paInt16

stop_recording = False

async def record_audio(ws):
    """Capture mic audio and send it over the WebSocket until user stops."""
    global stop_recording
    p = pyaudio.PyAudio()
    stream = p.open(format=FORMAT, channels=CHANNELS, rate=RATE, input=True, frames_per_buffer=CHUNK)

    print("Recording... Press Enter/Space to stop.\n")
    while not stop_recording:
        data = stream.read(CHUNK, exception_on_overflow=False)
        await ws.send(data)
        await asyncio.sleep(0)  # Allow event loop to handle incoming messages

    stream.stop_stream()
    stream.close()
    p.terminate()
    print("Stopped recording. Waiting for server response...")

async def receive_audio(ws):
    """Receive processed audio from the server and store it in a .wav file."""
    frames = []
    try:
        async for message in ws:
            frames.append(message)
    except websockets.ConnectionClosed:
        pass

    # Write frames to disk
    if frames:
        with wave.open(OUTPUT_FILE, 'wb') as wf:
            wf.setnchannels(CHANNELS)
            wf.setsampwidth(2)  # 16-bit
            wf.setframerate(RATE)
            wf.writeframes(b''.join(frames))
        print(f"Processed audio saved to: {OUTPUT_FILE}")

async def main():
    global stop_recording
    uri = f"ws://{HOST}:{PORT}"
    async with websockets.connect(uri) as ws:
        send_task = asyncio.create_task(record_audio(ws))
        receive_task = asyncio.create_task(receive_audio(ws))

        input()  # Block until you press Enter/Space
        stop_recording = True

        # Wait for send task to finish sending
        await send_task
        await ws.close()  # Done sending, signal the server
        # Wait for the final chunks of audio from server
        await receive_task

if __name__ == "__main__":
    asyncio.run(main())
