# Générateur de Mots Fléchés

Générateur automatique de grilles de mots fléchés français, distribué comme
librairie ESM. La grille est construite par un algorithme déterministe ; les
définitions des mots sont rédigées via l'API Gemini.

## Caractéristiques

- ✅ **100% remplissage** : aucune case lettre vide
- ✅ **Cases définitions** : chaque case définition porte 1 ou 2 mots
- ✅ **Flèches droites privilégiées** : le placement favorise les flèches `→` / `↓`
  (non coudées) via un matching à coût minimal
- ✅ **Déterministe** : une même `seedString` produit toujours la même grille
- ✅ **Définitions automatiques** : rédigées par Gemini (`gemini-2.5-flash`)
- ✅ **Corpus riche** : 18 327 mots français (2 à 11 lettres) + acronymes courants

## Installation

```bash
npm install
npm run build      # compile src/ → dist/
```

Le code source est en TypeScript ; `npm run build` (`tsc`) produit le JS et les
déclarations dans `dist/`. Le `dist/` est versionné pour qu'un consommateur
puisse utiliser le package sans étape de build.

## Utilisation

Le point d'entrée est le barrel `dist/index.js` (`import` ESM, pas `require`).

### Génération avec définitions

```javascript
import { generateWithDefinitions, getAllWords } from 'mots-fleches'

const words = getAllWords()                // tout le corpus, à plat
const result = await generateWithDefinitions(words, 9, 11, {
  seedString: '2026-05-22',                // optionnel — sinon seed aléatoire
})
```

`generateWithDefinitions` fait un appel à l'API Gemini (clé `GOOGLE_API_KEY`
lue via `dotenv`) — c'est une fonction asynchrone.

### Génération sans définitions

```javascript
import { generate, getAllWords } from 'mots-fleches'

const result = generate(getAllWords(), 9, 11, {
  templateAttempts: 500,   // nombre de templates essayés (défaut 500)
  nodeBudget: 50000,       // budget du solveur CSP (défaut 50000)
  seedString: 'ma-seed',   // optionnel
})
```

`generate` est synchrone et ne contacte pas Gemini (`result.definitions` est
alors absent). Les deux fonctions renvoient `null` si aucune grille valide n'est
trouvée.

### Corpus

```javascript
import { loadCorpus, getAllWords } from 'mots-fleches'

const corpus = loadCorpus()   // { easy: string[], medium: string[], hard: string[] }
const words  = getAllWords()  // les trois tiers concaténés
```

### Shape du résultat

```javascript
// GenerationResult
{
  grid: string[][],                      // grille 2D — lettres ou '#'
  defCells: Map<"r,c", ArrowClue[]>,      // case définition → liste de clues
  slots: Slot[],                         // emplacements des mots
  definitions?: Record<string, string>,  // MOT (majuscules) → définition
  wordCount: number,
  attempt: number,
  seed: number,
  seedString?: string,
  generationTimeMs: number,
}

// ArrowClue
{ word: string, dir: 'H' | 'V', arrow: string }
```

Les définitions sont indexées **par mot** (en majuscules), pas par position :
une case définition peut héberger deux mots de même direction.

Flèches possibles : `→` `↓` (droites), `↳` `↱` `⤵` `⤓` (coudées).

## CLI de test

```bash
npm run test:gen -- --rows=9 --cols=11 --seed="ma graine"
```

Affiche la grille générée et les définitions dans le terminal
(`tsx test-generator.ts`).

## Construction du corpus

`corpus.json` est déjà fourni. Pour le régénérer à partir de `Lexique383.tsv` :

```bash
npx tsx src/data/corpus.ts
```

Filtre Lexique383 (catégories NOM/ADJ/ADV/VER, fréquence, longueur 2-11),
ajoute les acronymes, et écrit `corpus.json` réparti en tiers easy/medium/hard.

## Architecture

### Pipeline de génération

1. **generateTemplate()** — pavage régulier avec placement aléatoire des cases définition
2. **extractSlots()** — extraction des emplacements horizontaux/verticaux (longueur 2-12)
3. **analyzeTemplate()** — validation de couverture + appariement slot ↔ case définition
   par flot à coût minimal (`matchCluesMinCost`, SPFA), qui privilégie les flèches droites
4. **solveFill()** — remplissage par backtracking CSP avec heuristique MRV
5. **assemble()** — rendu de la grille finale et des clues fléchées

### Préférence pour les flèches droites

L'appariement slot/case-définition est un flot à coût minimal : les arêtes vers
une position « flèche droite » ont un coût 0, les positions « coudées » un coût 1.
Parmi tous les appariements complets valides, l'algorithme choisit celui de coût
minimal — soit le maximum de flèches droites, sans jamais interdire les coudées
quand elles sont nécessaires.

## Intégration

Le package est consommé par `flopobot_v2` via une dépendance locale :

```json
"mots-fleches": "file:../mots-fleches"
```

Les deux dépôts doivent donc être côte à côte. `flopobot_v2` importe
`generateWithDefinitions` / `getAllWords` et génère la grille du jour via un cron.

## Dépendances

- **Node.js 18+** (ESM, top-level await)
- **`@google/genai`** — API Gemini pour les définitions
- **`dotenv`** — chargement de `GOOGLE_API_KEY`

## Licence

MIT
