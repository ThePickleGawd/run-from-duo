import { config } from "./config/defaults";
import express from "express";
import { setupOpenAIWebSocket } from "@services/openAIService";
import flashcardsRoute from "@routes/flashcards";
import quizRoute from "@routes/quiz";
import { extractApkg } from "@services/apkgService";

/*

Some TODO:
- Define functions that realtime API can call
  - ex. setHSK(), addInterests(), createQuiz()
  - Some will store in persistant game storage, while others may induce an event in Unity
- System prompt for HSK leveling, if the HSK level is undefined (otherwise base lessons on that)
- HTTP Methods to fetch cards. Optionally spin up the fetched data by sending it to GPT-4

Potential TODO:
- Delegate some prompts to GPT-4o, which is a lot cheaper
- The game itself where you shoot enemies and listen to audio should be GPT-4o, so define HTTP routes here
  - (The game state should be embedded in prompt so it know what HSK and user interests)
*/

// Setup express
const app = express();
app.use(express.json());

// Routes
app.use("/flashcards", flashcardsRoute);
app.use("/quiz", quizRoute);

app.listen(config.httpPort, () => {
  console.log(`Server running on http://localhost:${config.httpPort}`);
});

// Setup Realtime ChatGPT API
setupOpenAIWebSocket(config.webSocketPort, config.systemPrompt);
