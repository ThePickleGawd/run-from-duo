Some conventions:

- END_OF_SPEECH: Unity VR sends to Node.js
- END_OF_OUTPUT: Node.js sends to Unity VR
- Node.js sends OpenAI function calling response to Unity VR

How the data flows:

```
Websockets:

            (audio)
1. Unity VR =======> Node.js Backend
2. Node.js  =======> OpenAI
3. OpenAI   =======> Node.js
4. Node.js  =======> Unity VR

```
