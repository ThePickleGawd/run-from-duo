import express from "express";
import { getFlashcards } from "../services/apkgService";

const router = express.Router();

router.get("/:level", async (req, res) => {
  const level = req.params.level;
  const cards = await getFlashcards(Number(level));

  if (cards && cards.length > 0) {
    const randomCard = cards[Math.floor(Math.random() * cards.length)];
    res.json(randomCard);
  } else {
    res
      .status(404)
      .json({ error: "HSK level not found or no cards available" });
  }
});

export default router;
