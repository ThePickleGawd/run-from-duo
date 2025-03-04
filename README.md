# Run From Duo VR

It's Duolingo but there's a zombie outbreak in town! Use Chinese to find the green bird behind this mayhem, and perhaps you'll discover the key to saving everyone. And wait, Duo can talk?

[![Watch the Demo](https://img.youtube.com/vi/EEfnGCmj86o/0.jpg)](https://youtu.be/EEfnGCmj86o)

## Tech Stack

- Node.js backend with HTTP and WebSocket endpoints.
- OpenAI Realtime Audio API for live conversation
- Unity VR game

# Run Locally

## Backend

Ensure you have `Node.js` installed (tested with v22.11.0).

In the release section, download [`HSK.apkg`](https://github.com/ThePickleGawd/run-from-duo/releases/tag/hsk) and move it to `backend/flashcards/HSK.apkg`. Running the server will automatically extract it's contents.

You'll also need to create a `.env` file in the backend base directory with an OpenAI API key.

```.env
OPENAI_API_KEY=<YOUR KEY HERE>
```

Run the server

```bash
cd backend

# Install dependencies
npm install

# Run local server
npm run dev
```

To test the backend, there are some small Python files to play with. Make sure you're server is running on localhost. Try asking Duo for some ammo or weapons.

```bash
cd backend/tests

# Optionally create a .venv file
python -m venv .venv
pip install websockets pyaudio

# Print microphone settings (useful to debugging the following script)
python test_mic.py

# Talk to backend with websockets. Talk for 2 seconds at a time. Make sure mic index is correct in code.
python test_ws.py
```

## Unity VR

Install `Unity 6 LTS` via [Unity Hub](https://unity.com/unity-hub). (currently `Unity 6000.0.34f1`). Add the project to Unity Hub and open it.

To build the game and load it to your Meta Quest VR headset:

1. Connect headset via USB to your computer
2. Open the build menu (`File->Build Settings`). Change Build Target to Android
3. Click the `Build and Run` button on the bottom right of the build menu
4. IMPORTANT: In GameManager.cs, set the base url of your backend server.

Alternatively, sideload the [`game.apk`](https://github.com/ThePickleGawd/run-from-duo/releases/tag/apk) file in the release section. You can look up how to sideload it onto your VR headset.
