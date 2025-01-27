import asyncio
import websockets
import pyaudio

CHUNK = 1024
RATE = 44100
FORMAT = pyaudio.paInt16
CHANNELS = 1
DEVICE_INDEX = 1
OUTPUT_FILE = "processed_output.wav"

async def record_and_send(websocket):
    """
    Records mic data and sends it to the server until user indicates end of speech.
    """
    p = pyaudio.PyAudio()
    stream = p.open(
        format=FORMAT,
        channels=CHANNELS,
        rate=RATE,
        input=True,
        input_device_index=DEVICE_INDEX,
        frames_per_buffer=CHUNK
    )

    print("Recording... Press ENTER to stop recording.")
    
    # We'll run the mic loop in a Task so we can interrupt it easily
    async def mic_loop():
        while True:
            data = stream.read(CHUNK)
            await websocket.send(data)

    mic_task = asyncio.create_task(mic_loop())
    
    # Wait for user to press Enter (blocking call in a thread)
    loop = asyncio.get_running_loop()
    await loop.run_in_executor(None, input)
    
    # User pressed Enter -> stop mic loop
    mic_task.cancel()
    try:
        await mic_task
    except asyncio.CancelledError:
        pass

    # Send END_OF_SPEECH
    await websocket.send("END_OF_SPEECH")
    print("Sent END_OF_SPEECH, no more mic data sent.")
    
    stream.stop_stream()
    stream.close()
    p.terminate()

async def receive_and_write(websocket):
    """
    Receives audio from the server and writes to a file until server sends END_OF_OUTPUT.
    """
    with open(OUTPUT_FILE, 'wb') as f:
        while True:
            message = await websocket.recv()
            
            if message == "END_OF_OUTPUT":
                print("Received END_OF_OUTPUT, stopping reception.")
                break
            elif isinstance(message, bytes):
                # It's audio data, write to file
                f.write(message)

async def main():
    uri = "ws://localhost:8000"
    async with websockets.connect(uri) as websocket:
        # Run record_and_send() and receive_and_write() in parallel
        send_task = asyncio.create_task(record_and_send(websocket))
        receive_task = asyncio.create_task(receive_and_write(websocket))

        # Wait for both tasks to complete
        await asyncio.gather(send_task, receive_task)

if __name__ == "__main__":
    asyncio.run(main())
