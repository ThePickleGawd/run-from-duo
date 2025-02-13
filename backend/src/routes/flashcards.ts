import express from "express";
import { getFlashcards } from "../services/apkgService";

const router = express.Router();

router.get("/:level", async (req, res) => {
  const level = req.params.level;
  const cards = await getFlashcards(level);

  if (cards) {
    res.json(cards);
  } else {
    res.status(404).json({ error: "HSK level not found" });
  }
});

export default router;
