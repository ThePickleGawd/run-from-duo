import asyncio
import websockets
import pyaudio
import wave

CHUNK = 1024
RATE = 24000
FORMAT = pyaudio.paInt16
CHANNELS = 1
DEVICE_INDEX = 0
RECORD_SECONDS = 3  # Duration for each session

async def record_and_send(websocket):
    """
    Records mic input for RECORD_SECONDS seconds and sends it to the server.
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
    
    print(f"Recording for {RECORD_SECONDS} seconds...")
    for _ in range(int(RATE / CHUNK * RECORD_SECONDS)):
        data = stream.read(CHUNK)
        await websocket.send(data)
    
    # Signal the end of mic input
    await websocket.send("END_OF_SPEECH")
    print("Sent END_OF_SPEECH.")
    
    stream.stop_stream()
    stream.close()
    p.terminate()

async def receive_and_write(websocket, output_file):
    """
    Receives processed audio data from the server until 'END_OF_OUTPUT',
    writes it to a valid WAV file with the proper header, and plays audio.
    """
    import wave
    import pyaudio

    # Initialize PyAudio
    p = pyaudio.PyAudio()
    sample_width = p.get_sample_size(FORMAT)

    # Set up WAV file for writing
    wf = wave.open(output_file, 'wb')
    wf.setnchannels(CHANNELS)
    wf.setsampwidth(sample_width)
    wf.setframerate(RATE)

    # Set up audio playback stream
    stream = p.open(
        format=FORMAT,
        channels=CHANNELS,
        rate=RATE,
        output=True,
        frames_per_buffer=CHUNK
    )

    while True:
        message = await websocket.recv()
        if message == "END_OF_OUTPUT":
            print("Received END_OF_OUTPUT, ending session.")
            break
        elif isinstance(message, bytes):
            # Write to file and play audio
            wf.writeframes(message)
            stream.write(message)

    wf.close()
    stream.stop_stream()
    stream.close()
    p.terminate()


async def session(websocket, session_number):
    output_filename = f"output/output_{session_number}.wav"
    print(f"\nStarting session {session_number} (output file: {output_filename})")
    
    # Run sending and receiving concurrently
    send_task = asyncio.create_task(record_and_send(websocket))
    receive_task = asyncio.create_task(receive_and_write(websocket, output_filename))
    await asyncio.gather(send_task, receive_task)
    print(f"Session {session_number} complete.")

async def main():
    uri = "ws://localhost:8000"
    async with websockets.connect(uri) as websocket:
        print("Waiting 2 seconds for server setup...")
        await asyncio.sleep(2)
        
        session_number = 1
        while True:
            await session(websocket, session_number)
            session_number += 1

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\nProgram terminated by user.")
