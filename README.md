# Run From Duo VR

It's Duolingo but there's a zombie outbreak in town! Use Chinese to find the green bird behind this mayhem, and perhaps you'll discover the key to saving everyone. And wait, Duo can talk?

## Tech Stack

- Node.js backend with HTTP and WebSocket endpoints.
- OpenAI Realtime Audio API for live conversation
- Unity VR game

# Run Locally

## Backend

Ensure you have `Node.js` installed (tested with v22.11.0).

In the release section, download `HSK.apkg` and move it to `backend/flashcards/HSK.apkg`. Running the server will automatically extract it's contents.

```bash
cd backend

# Install dependencies
npm install

# Run local server
npm run dev
```

## Unity VR

Install `Unity 6 LTS` via [Unity Hub](https://unity.com/unity-hub). (currently `Unity 6000.0.34f1`). Add the project to Unity Hub and open it.

To build the game and load it to your Meta Quest VR headset:

1. Connect headset via USB to your computer
2. Open the build menu (`File->Build Settings`). Change Build Target to Android
3. Click the `Build and Run` button on the bottom right of the build menu

Alternatively, the `.apk` file will soon be published to the release section. You can look up how to sideload it onto your VR headset.
