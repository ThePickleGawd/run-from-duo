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

export const getFlashcards = async (level: string) => {
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
      front: fields[0],
      back: fields[1] || "",
      promptString: fields[2] || null,
    };
  });

  db.close();
  return flashcards;
};

// Ensure data is extracted before usage
extractApkg();
