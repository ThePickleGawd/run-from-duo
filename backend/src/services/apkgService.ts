import fs from "fs";
import unzipper from "unzipper";
import Database from "better-sqlite3";

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

export const getFlashcards = async () => {
  if (!fs.existsSync(dbPath)) {
    console.error("Database not found. Run extractApkg() first.");
    return null;
  }

  const db = new Database(dbPath, { readonly: true });
  const stmt = db.prepare("SELECT id, flds FROM notes");
  const rows = stmt.all() as { id: number; flds: string }[];

  const flashcards = rows.map((row) => {
    const fields = row.flds.split("\x1f"); // Anki fields separator
    return {
      id: row.id,
      hanzi: fields[1] || "", // Chinese character
      pinyin: fields[3] || "", // Pinyin with tone marks
      pinyinNumbers: fields[4] || "", // Pinyin with numbers
      definition: fields[5] || "", // English definition
      partOfSpeech: fields[6] || "", // Part of speech (e.g., noun, verb)
      sound: fields[7] || "", // Audio file
      sentence: fields[10] || "", // Example sentence (simplified)
      sentenceTraditional: fields[11] || "", // Example sentence (traditional)
      sentenceBlank: fields[12] || "", // Cloze sentence (simplified)
      sentenceBlankTraditional: fields[13] || "", // Cloze sentence (traditional)
      sentencePinyin: fields[14] || "", // Example sentence with pinyin
      sentencePinyinNumbers: fields[15] || "", // Example sentence with numbered pinyin
      sentenceEnglish: fields[16] || "", // Example sentence in English
      sentenceSound: fields[17] || "", // Example sentence audio
      image: fields[18] || "", // Associated image
    };
  });

  db.close();
  return flashcards;
};

// Ensure data is extracted before usage
extractApkg();
