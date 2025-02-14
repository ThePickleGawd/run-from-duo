import fs from "fs";
import unzipper from "unzipper";
import Database from "better-sqlite3";

/*
HSK1: 1-150
HSK2: 151-301
HSK3: 302-601
HSK4: 602-1201
HSK5: 1202-2501
HSK6: 2501-5001
*/

// Relative to /backend
const apkgPath = "flashcards/HSK.apkg";
const outputDir = "flashcards/HSK_db";
const dbPath = `${outputDir}/collection.anki2`;

export const extractApkg = async () => {
  if (!fs.existsSync(outputDir)) {
    await fs
      .createReadStream(apkgPath)
      .pipe(unzipper.Extract({ path: outputDir }))
      .promise();
    console.log("Extraction complete!");
  } else {
    console.log("Extraction skipped: output directory already exists.");
  }
};

const getHSKLevel = (index: number) => {
  if (index >= 1 && index <= 150) return 1;
  if (index >= 151 && index <= 301) return 2;
  if (index >= 302 && index <= 601) return 3;
  if (index >= 602 && index <= 1201) return 4;
  if (index >= 1202 && index <= 2501) return 5;
  if (index >= 2502 && index <= 5001) return 6;
  return null; // Out of range
};

export const getFlashcards = async (level: number) => {
  if (!fs.existsSync(dbPath)) {
    console.error("Database not found. Run extractApkg() first.");
    return null;
  }

  const db = new Database(dbPath, { readonly: true });
  const stmt = db.prepare("SELECT id, flds FROM notes");
  const rows = stmt.all() as { id: number; flds: string }[];

  const flashcards = rows.map((row, index) => {
    const fields = row.flds.split("\x1f"); // Anki fields separator
    return {
      id: row.id,
      simplified: fields[1] || "", // Simplified Chinese character
      traditional: fields[2] || "", // Traditional Chinese character
      pinyin1: fields[3] || "", // Pinyin with tone marks
      pinyin2: fields[4] || "", // Pinyin with numbers
      meaning: fields[5] || "", // English meaning
      partOfSpeech: fields[6] || "", // Part of speech (e.g., noun, verb)
      audio: fields[7] || "", // Audio file
      homophone: fields[8] || "", // Homophone
      homograph: fields[9] || "", // Homograph
      sentenceSimplified: fields[10] || "", // Example sentence (simplified)
      sentenceTraditional: fields[11] || "", // Example sentence (traditional)
      sentenceSimplifiedCloze: fields[12] || "", // Cloze sentence (simplified)
      sentenceTraditionalCloze: fields[13] || "", // Cloze sentence (traditional)
      sentencePinyin1: fields[14] || "", // Example sentence with pinyin
      sentencePinyin2: fields[15] || "", // Example sentence with numbered pinyin
      sentenceMeaning: fields[16] || "", // Example sentence meaning
      sentenceAudio: fields[17] || "", // Example sentence audio
      sentenceImage: fields[18] || "", // Associated image
      hskLevel: getHSKLevel(index + 1), // Determine HSK level
    };
  });

  db.close();

  return level
    ? flashcards.filter((card) => card.hskLevel === level)
    : flashcards;
};

// Ensure data is extracted before usage
extractApkg();
