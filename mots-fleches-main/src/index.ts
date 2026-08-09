import { readFileSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, resolve } from 'path';

export { generate, generateWithDefinitions, clueKey } from './utils/generator.js';
export type { GenerationResult, ArrowClue, Slot, PlacedWord, WordIndex } from './utils/generator.js';
export { generateDefinitions } from './utils/definitions-gen.js';
export type { DefinitionEntry } from './utils/definitions-gen.js';

export interface Corpus {
  easy: string[];
  medium: string[];
  hard: string[];
}

let cachedCorpus: Corpus | null = null;

export function loadCorpus(): Corpus {
  if (cachedCorpus) return cachedCorpus;
  const here = dirname(fileURLToPath(import.meta.url));
  const corpusPath = resolve(here, '..', 'corpus.json');
  cachedCorpus = JSON.parse(readFileSync(corpusPath, 'utf-8')) as Corpus;
  return cachedCorpus;
}

export function getAllWords(): string[] {
  const c = loadCorpus();
  return [...c.easy, ...c.medium, ...c.hard];
}

export function getEasyWords(): string[] {
    const c = loadCorpus();
    return [...c.easy];
}

export function getEasyAndMediumWords(): string[] {
    const c = loadCorpus();
    return [...c.easy, ...c.medium];
}