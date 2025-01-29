import pyaudio
import wave

# Settings
FORMAT = pyaudio.paInt16  # 16-bit audio format
CHANNELS = 1  # Mono input (match your microphone's input channels)
RATE = 24000  # Sample rate (standard for audio)
CHUNK = 1024  # Buffer size
RECORD_SECONDS = 5  # Duration of recording
OUTPUT_FILENAME = "mic_test.wav"  # Output file name

# Initialize PyAudio
p = pyaudio.PyAudio()

# List devices to confirm the correct microphone
print("Available input devices:")
for i in range(p.get_device_count()):
    device_info = p.get_device_info_by_index(i)
    print(f"Device {i}: {device_info['name']} - Input Channels: {device_info['maxInputChannels']}")

# Choose the device index (update this if needed)
DEVICE_INDEX = int(input("Select the index (number only) of you input device: "))  # Update to the correct device index

# Open the audio stream
try:
    stream = p.open(format=FORMAT,
                    channels=CHANNELS,
                    rate=RATE,
                    input=True,
                    input_device_index=DEVICE_INDEX,
                    frames_per_buffer=CHUNK)
    print("Recording...")
    
    # Record audio
    frames = []
    for _ in range(0, int(RATE / CHUNK * RECORD_SECONDS)):
        data = stream.read(CHUNK)
        frames.append(data)
    
    print("Recording finished.")

    # Stop and close the stream
    stream.stop_stream()
    stream.close()
    p.terminate()

    # Save the recorded data to a WAV file
    with wave.open(OUTPUT_FILENAME, 'wb') as wf:
        wf.setnchannels(CHANNELS)
        wf.setsampwidth(p.get_sample_size(FORMAT))
        wf.setframerate(RATE)
        wf.writeframes(b''.join(frames))
    
    print(f"Audio saved to {OUTPUT_FILENAME}. Play the file to test your microphone.")
except Exception as e:
    print(f"An error occurred: {e}")
    p.terminate()
