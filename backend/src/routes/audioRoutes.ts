import { Router } from 'express';
import multer from 'multer';
import { uploadAudio, streamAudio } from '../controllers/audioController';

const router = Router();
const upload = multer(); // Multer instance for handling file uploads

router.post('/upload', upload.single('audio'), uploadAudio);
router.get('/stream', streamAudio);

export { router as audioRouter };

// src/controllers/audioController.ts
import { Request, Response } from 'express';
import { processAudioStream } from '../services/audioService';
import { callOpenAI } from '../services/openAIService';

export const uploadAudio = async (req: Request, res: Response): Promise<void> => {
  try {
    const file = req.file;
    if (!file) {
      res.status(400).json({ success: false, error: 'No file uploaded' });
      return;
    }

    const response = await processAudioStream(file.buffer);
    res.status(200).json({ success: true, data: response });
  } catch (error) {
    console.error(error);
    res.status(500).json({ success: false, error: 'Internal Server Error' });
  }
};