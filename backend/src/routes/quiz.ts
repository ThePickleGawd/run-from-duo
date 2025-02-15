import express from "express";
import { getFlashcards } from "../services/apkgService";
import { Flashcard } from "../types";

const router = express.Router();

router.get("/:level", async (req, res) => {
  const level = req.params.level;
  const cards = await getFlashcards(Number(level));

  if (!cards || cards.length < 4) {
    res
      .status(404)
      .json({ error: "Not enough flashcards available for a quiz" });
  }

  // Select 4 unique random cards
  const selectedCards: Flashcard[] = [];
  while (selectedCards.length < 4) {
    const randomCard = cards[Math.floor(Math.random() * cards.length)];
    if (!selectedCards.some((card) => card.id === randomCard.id)) {
      selectedCards.push(randomCard);
    }
  }

  // Pick one correct answer randomly from the selected cards
  const correctIndex = Math.floor(Math.random() * 4);
  const correctCard = selectedCards[correctIndex];

  // Create the quiz question
  const quiz = {
    prompt: `${correctCard.meaning}`,
    options: selectedCards.map((card) => ({
      text: `${card.simplified} (${card.pinyin1})`,
      isCorrect: card.id === correctCard.id,
    })),
  };

  res.json(quiz);
});

export default router;
