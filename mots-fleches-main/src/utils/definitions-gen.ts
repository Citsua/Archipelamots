import "dotenv/config";
import { GoogleGenAI, Type } from "@google/genai";

const ai = new GoogleGenAI({
  apiKey: process.env.GOOGLE_API_KEY,
});

const MODEL = "gemini-2.5-flash";

const PROMPT = `Tu es un cruciverbiste expert. Pour chaque mot fourni, génère une définition de mots fléchés en français, de difficulté "force 3" (niveau un peu avancé, définitions légèrement imagées).

Règles strictes :
- Définition courte (idéalement 2 à 6 mots), sans article inutile, style télégraphique.
- Pas de majuscule initiale sauf nom propre, pas de point final.
- Ne jamais utiliser le mot à définir, ni un mot de la même famille, ni le traduire.
- Respecter le nombre, le genre et la nature grammaticale du mot.
- Pour un verbe, donner un synonyme à l'infinitif ; pour un pluriel, indiquer le pluriel ; pour un participe passé, donner une définition au participe passé.
- Privilégier les définitions classiques type "Larousse des mots fléchés".`;

export interface DefinitionEntry {
  word: string;
  definition: string;
}

export async function generateDefinitions(
  words: string[]
): Promise<Record<string, string>> {
  const uniqueWords = Array.from(
    new Set(words.map((w) => w.toUpperCase().trim()).filter(Boolean))
  );

  if (uniqueWords.length === 0) return {};

  const response = await ai.models.generateContent({
    model: MODEL,
    contents: `${PROMPT}\n\nMots à définir :\n${uniqueWords.join(", ")}`,
    config: {
      responseMimeType: "application/json",
      responseSchema: {
        type: Type.ARRAY,
        items: {
          type: Type.OBJECT,
          properties: {
            word: {
              type: Type.STRING,
              description: "Le mot à définir, en majuscules, identique à l'entrée.",
            },
            definition: {
              type: Type.STRING,
              description: "Définition de mots fléchés force 3, courte et en français.",
            },
          },
          required: ["word", "definition"],
          propertyOrdering: ["word", "definition"],
        },
      },
    },
  });

  const raw = response.text;
  if (!raw) throw new Error("Réponse vide de l'API Gemini.");

  const entries = JSON.parse(raw) as DefinitionEntry[];

  const result: Record<string, string> = {};
  for (const { word, definition } of entries) {
    result[word.toUpperCase().trim()] = definition.trim();
  }
  return result;
}

if (import.meta.url === `file://${process.argv[1]}`) {
  const words = process.argv.slice(2);
  const input = words.length > 0 ? words : ["EXEMPLE", "TEST", "GENERATION"];

  generateDefinitions(input)
    .then((defs) => {
      console.log(JSON.stringify(defs, null, 2));
    })
    .catch((err) => {
      console.error("Erreur :", err);
      process.exit(1);
    });
}
