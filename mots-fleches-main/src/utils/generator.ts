import { generateDefinitions } from './definitions-gen.js';

export interface Slot {
  r: number;
  c: number;
  dir: 'H' | 'V';
  len: number;
  cells: { r: number; c: number }[];
}

export interface PlacedWord {
  text: string;
  r: number;
  c: number;
  dir: 'H' | 'V';
  len: number;
}

export interface WordIndex {
  byLength: Map<number, string[]>;
  byLetterPos: Map<string, string[]>;
}

export interface ArrowClue {
  word: string;
  dir: 'H' | 'V';
  arrow: string;
}

export interface GenerationResult {
  grid: string[][];
  defCells: Map<string, ArrowClue[]>;
  slots: Slot[];
  wordCount: number;
  attempt: number;
  seed: number;
  seedString?: string;
  generationTimeMs: number;
  definitions?: Record<string, string>;
}

export function clueKey(r: number, c: number, dir: 'H' | 'V'): string {
  return `${r},${c},${dir}`;
}

export function buildIndex(words: string[]): WordIndex {
  const byLength = new Map<number, string[]>();
  const byLetterPos = new Map<string, string[]>();

  const uniqueWords = Array.from(new Set(words.map(w => w.toUpperCase().trim())));

  for (const w of uniqueWords) {
    const len = w.length;
    if (!byLength.has(len)) byLength.set(len, []);
    byLength.get(len)!.push(w);

    for (let i = 0; i < len; i++) {
      const key = `${len},${i},${w[i]}`;
      if (!byLetterPos.has(key)) {
        byLetterPos.set(key, []);
      }
      byLetterPos.get(key)!.push(w);
    }
  }
  return { byLength, byLetterPos };
}

export function matchPattern(index: WordIndex, length: number, pattern: (string | null)[]): string[] {
  const constraints: [number, string][] = [];
  for (let i = 0; i < length; i++) {
    if (pattern[i] !== null) {
      constraints.push([i, pattern[i] as string]);
    }
  }

  if (constraints.length === 0) {
    return index.byLength.get(length) || [];
  }

  let bestPos = -1;
  let minSize = Infinity;
  let bestList: string[] | null = null;

  for (const [pos, letter] of constraints) {
    const list = index.byLetterPos.get(`${length},${pos},${letter}`);
    if (!list || list.length === 0) return [];
    if (list.length < minSize) {
      minSize = list.length;
      bestList = list;
      bestPos = pos;
    }
  }

  if (!bestList || minSize === 0) return [];

  const numConstraints = constraints.length;
  if (numConstraints === 1) {
    return bestList;
  }

  const otherConstraints: [number, string][] = [];
  for (let idx = 0; idx < numConstraints; idx++) {
    if (constraints[idx][0] !== bestPos) {
      otherConstraints.push(constraints[idx]);
    }
  }
  const numOthers = otherConstraints.length;

  const result: string[] = [];
  for (let i = 0; i < bestList.length; i++) {
    const w = bestList[i];
    let matches = true;
    for (let j = 0; j < numOthers; j++) {
      const [pos, letter] = otherConstraints[j];
      if (w[pos] !== letter) {
        matches = false;
        break;
      }
    }
    if (matches) {
      result.push(w);
    }
  }
  return result;
}

export function hashSeedString(seedString: string): number {
  let hash = 2166136261;
  for (let i = 0; i < seedString.length; i++) {
    hash ^= seedString.charCodeAt(i);
    hash = Math.imul(hash, 16777619);
  }
  return hash >>> 0;
}

export function randomSeedString(length = 12): string {
  const chars = '0123456789abcdefghijklmnopqrstuvwxyz';
  let result = '';
  for (let i = 0; i < length; i++) {
    result += chars[Math.floor(Math.random() * chars.length)];
  }
  return result;
}

export function makeRng(seed: number): () => number {
  let s = seed >>> 0;
  return () => {
    s |= 0; s = (s + 0x6D2B79F5) | 0;
    let t = Math.imul(s ^ (s >>> 15), 1 | s);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

export function shuffle<T>(arr: T[], rng: () => number): T[] {
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(rng() * (i + 1));
    const temp = arr[i];
    arr[i] = arr[j];
    arr[j] = temp;
  }
  return arr;
}

const MIN_SLOT = 2;
const MAX_SLOT = 12;
const MAX_CONSEC = 2;
const TEMPLATE_MIN_SLOT = 3;
const TEMPLATE_MAX_SLOT = 5;
const ADJACENT_DEF_PROB = 0.25;

function randInt(rng: () => number, min: number, max: number): number {
  return min + Math.floor(rng() * (max - min + 1));
}

function applyBorderPattern(grid: string[][]): string[][] {
  const R = grid.length, C = grid[0].length;
  if (R === 0 || C === 0) return grid;

  for (let c = 0; c < C; c++) grid[0][c] = (c % 2 === 0) ? '#' : '.';
  for (let r = 0; r < R; r++) grid[r][0] = (r % 2 === 0) ? '#' : '.';

  if (R > 1) {
    for (let c = 1; c < C; c += 2) grid[1][c] = '.';
  }
  if (C > 1) {
    for (let r = 1; r < R; r += 2) grid[r][1] = '.';
  }
  return grid;
}

function placeRowDefinitions(grid: string[][], r: number, rng: () => number, minGap: number, maxGap: number) {
  const C = grid[0].length;
  const G = randInt(rng, minGap, maxGap);
  const off = Math.floor(rng() * G);
  for (let c = G + off; c <= C - 2; c += G) {
    if (C - 1 - c < minGap) break;
    grid[r][c] = '#';
    if (rng() < ADJACENT_DEF_PROB && c + 1 <= C - 2 && grid[r][c + 1] === '.') {
      grid[r][c + 1] = '#';
      c += 1;
    }
  }
}

export function generateTemplate(R: number, C: number, rng: () => number): string[][] {
  const grid = Array.from({ length: R }, () => Array(C).fill('.'));
  
  for (let r = 0; r < R; r++) {
    placeRowDefinitions(grid, r, rng, TEMPLATE_MIN_SLOT, TEMPLATE_MAX_SLOT);
  }
  return applyBorderPattern(grid);
}

export function makeSlot(r: number, c: number, dir: 'H' | 'V', len: number): Slot {
  const [dr, dc] = dir === 'H' ? [0, 1] : [1, 0];
  const cells = [];
  for (let i = 0; i < len; i++) {
    cells.push({ r: r + dr * i, c: c + dc * i });
  }
  return { r, c, dir, len, cells };
}

export function extractSlots(grid: string[][]): Slot[] {
  const R = grid.length, C = grid[0].length;
  const slots: Slot[] = [];

  for (let r = 0; r < R; r++) {
    let start = -1;
    for (let c = 0; c <= C; c++) {
      const white = c < C && grid[r][c] === '.';
      if (white && start === -1) start = c;
      if (!white && start !== -1) {
        const len = c - start;
        if (len >= MIN_SLOT && len <= MAX_SLOT) {
          slots.push(makeSlot(r, start, 'H', len));
        }
        start = -1;
      }
    }
  }

  for (let c = 0; c < C; c++) {
    let start = -1;
    for (let r = 0; r <= R; r++) {
      const white = r < R && grid[r][c] === '.';
      if (white && start === -1) start = r;
      if (!white && start !== -1) {
        const len = r - start;
        if (len >= MIN_SLOT && len <= MAX_SLOT) {
          slots.push(makeSlot(start, c, 'V', len));
        }
        start = -1;
      }
    }
  }
  return slots;
}

export function clueCandidates(grid: string[][], s: Slot): [number, number, string][] {
  const R = grid.length, C = grid[0].length;
  const raw: [number, number, string][] = s.dir === 'H'
    ? [[s.r, s.c - 1, '→'], [s.r - 1, s.c, '↳'], [s.r + 1, s.c, '↱']]
    : [[s.r - 1, s.c, '↓'], [s.r, s.c - 1, '⤵'], [s.r, s.c + 1, '⤓']];
  return raw.filter(([r, c]) =>
    r >= 0 && r < R && c >= 0 && c < C && grid[r][c] === '#');
}

export function isStraightArrow(arrow: string): boolean {
  return arrow === '→' || arrow === '↓';
}

export function matchCluesMinCost(
  S: number,
  D: number,
  slotCands: { def: number; cost: number }[][],
  cap: number,
): number[] | null {
  const NS = 2;
  const sNode = (i: number) => NS + i;
  const dNode = (j: number) => NS + S + j;
  const N = NS + S + D;
  const SS = N;
  const TT = N + 1;
  const V = N + 2;

  const to: number[] = [];
  const cap_: number[] = [];
  const cost_: number[] = [];
  const nxt: number[] = [];
  const head = new Array(V).fill(-1);

  const add = (u: number, v: number, c: number, k: number) => {
    to.push(v); cap_.push(c); cost_.push(k); nxt.push(head[u]); head[u] = to.length - 1;
    to.push(u); cap_.push(0); cost_.push(-k); nxt.push(head[v]); head[v] = to.length - 1;
  };

  const exc = new Array(V).fill(0);
  const addLU = (u: number, v: number, L: number, U: number, k: number) => {
    if (U > L) add(u, v, U - L, k);
    exc[v] += L;
    exc[u] -= L;
  };

  for (let i = 0; i < S; i++) addLU(0, sNode(i), 1, 1, 0);
  for (let i = 0; i < S; i++) {
    for (const cand of slotCands[i]) {
      addLU(sNode(i), dNode(cand.def), 0, 1, cand.cost);
    }
  }
  for (let j = 0; j < D; j++) addLU(dNode(j), 1, 1, cap, 0);
  add(1, 0, 1e9, 0);

  let total = 0;
  for (let n = 0; n < N; n++) {
    if (exc[n] > 0) {
      add(SS, n, exc[n], 0);
      total += exc[n];
    } else if (exc[n] < 0) {
      add(n, TT, -exc[n], 0);
    }
  }

  const dist = new Array<number>(V);
  const inQueue = new Array<boolean>(V).fill(false);
  const prevEdge = new Array<number>(V);

  const spfa = (): boolean => {
    for (let i = 0; i < V; i++) {
      dist[i] = Infinity;
      inQueue[i] = false;
      prevEdge[i] = -1;
    }
    dist[SS] = 0;
    const q: number[] = [SS];
    inQueue[SS] = true;
    let qi = 0;
    while (qi < q.length) {
      const u = q[qi++];
      inQueue[u] = false;
      for (let e = head[u]; e !== -1; e = nxt[e]) {
        const v = to[e];
        if (cap_[e] > 0 && dist[u] + cost_[e] < dist[v]) {
          dist[v] = dist[u] + cost_[e];
          prevEdge[v] = e;
          if (!inQueue[v]) {
            q.push(v);
            inQueue[v] = true;
          }
        }
      }
    }
    return dist[TT] < Infinity;
  };

  let flow = 0;
  while (spfa()) {
    let bottleneck = Infinity;
    for (let v = TT; v !== SS;) {
      const e = prevEdge[v];
      if (cap_[e] < bottleneck) bottleneck = cap_[e];
      v = to[e ^ 1];
    }
    for (let v = TT; v !== SS;) {
      const e = prevEdge[v];
      cap_[e] -= bottleneck;
      cap_[e ^ 1] += bottleneck;
      v = to[e ^ 1];
    }
    flow += bottleneck;
  }

  if (flow !== total) return null;

  const assign = new Array(S).fill(-1);
  for (let i = 0; i < S; i++) {
    for (let e = head[sNode(i)]; e !== -1; e = nxt[e]) {
      const v = to[e];
      if (e % 2 === 0 && v >= NS + S && v < NS + S + D && cap_[e] === 0) {
        assign[i] = v - (NS + S);
        break;
      }
    }
  }
  return assign;
}

export function matchClues(S: number, D: number, slotCands: number[][], cap: number): number[] | null {
  const NS = 2;
  const sNode = (i: number) => NS + i;
  const dNode = (j: number) => NS + S + j;
  const N = NS + S + D;
  const SS = N;
  const TT = N + 1;
  const V = N + 2;

  const to: number[] = [];
  const cap_: number[] = [];
  const nxt: number[] = [];
  const head = new Array(V).fill(-1);

  const add = (u: number, v: number, c: number) => {
    to.push(v); cap_.push(c); nxt.push(head[u]); head[u] = to.length - 1;
    to.push(u); cap_.push(0); nxt.push(head[v]); head[v] = to.length - 1;
  };

  const exc = new Array(V).fill(0);
  const addLU = (u: number, v: number, L: number, U: number) => {
    if (U > L) add(u, v, U - L);
    exc[v] += L;
    exc[u] -= L;
  };

  for (let i = 0; i < S; i++) addLU(0, sNode(i), 1, 1);
  for (let i = 0; i < S; i++) {
    for (const j of slotCands[i]) {
      addLU(sNode(i), dNode(j), 0, 1);
    }
  }
  for (let j = 0; j < D; j++) addLU(dNode(j), 1, 1, cap);
  add(1, 0, 1e9);

  let total = 0;
  for (let n = 0; n < N; n++) {
    if (exc[n] > 0) {
      add(SS, n, exc[n]);
      total += exc[n];
    } else if (exc[n] < 0) {
      add(n, TT, -exc[n]);
    }
  }

  const level = new Array(V);
  const it = new Array(V);

  const bfs = () => {
    level.fill(-1);
    level[SS] = 0;
    const q = [SS];
    for (let qi = 0; qi < q.length; qi++) {
      const u = q[qi];
      for (let e = head[u]; e !== -1; e = nxt[e]) {
        if (cap_[e] > 0 && level[to[e]] < 0) {
          level[to[e]] = level[u] + 1;
          q.push(to[e]);
        }
      }
    }
    return level[TT] >= 0;
  };

  const dfs = (u: number, f: number): number => {
    if (u === TT) return f;
    for (; it[u] !== -1; it[u] = nxt[it[u]]) {
      const e = it[u], v = to[e];
      if (cap_[e] > 0 && level[v] === level[u] + 1) {
        const d = dfs(v, Math.min(f, cap_[e]));
        if (d > 0) {
          cap_[e] -= d;
          cap_[e ^ 1] += d;
          return d;
        }
      }
    }
    return 0;
  };

  let flow = 0;
  while (bfs()) {
    for (let n = 0; n < V; n++) it[n] = head[n];
    let f;
    while ((f = dfs(SS, 1e9)) > 0) flow += f;
  }
  if (flow !== total) return null;

  const assign = new Array(S).fill(-1);
  for (let i = 0; i < S; i++) {
    for (let e = head[sNode(i)]; e !== -1; e = nxt[e]) {
      const v = to[e];
      if (e % 2 === 0 && v >= NS + S && v < NS + S + D && cap_[e] === 0) {
        assign[i] = v - (NS + S);
        break;
      }
    }
  }
  return assign;
}

export function analyzeTemplate(grid: string[][], index?: WordIndex): {
  ok: boolean;
  why?: string;
  slots?: Slot[];
  defKeys?: { r: number; c: number }[];
  assign?: number[];
} {
  const R = grid.length, C = grid[0].length;
  const defKeys: { r: number; c: number }[] = [];
  const defIndex = new Map<string, number>();

  for (let r = 0; r < R; r++) {
    for (let c = 0; c < C; c++) {
      if (grid[r][c] === '#') {
        defIndex.set(`${r},${c}`, defKeys.length);
        defKeys.push({ r, c });
      }
    }
  }

  const slots = extractSlots(grid).filter(s => clueCandidates(grid, s).length > 0);
  if (slots.length === 0) return { ok: false, why: 'noslots' };

  if (index) {
    for (const s of slots) {
      const list = index.byLength.get(s.len);
      if (!list || list.length === 0) {
        return { ok: false, why: `longueur_vide_${s.len}` };
      }
    }
  }

  for (let r = 0; r < R; r++) {
    let run = 0;
    for (let c = 0; c < C; c++) {
      run = grid[r][c] === '#' ? run + 1 : 0;
      if (run > MAX_CONSEC) return { ok: false, why: 'consec' };
    }
  }
  for (let c = 0; c < C; c++) {
    let run = 0;
    for (let r = 0; r < R; r++) {
      run = grid[r][c] === '#' ? run + 1 : 0;
      if (run > MAX_CONSEC) return { ok: false, why: 'consec' };
    }
  }

  const covered = Array.from({ length: R }, () => Array(C).fill(false));
  for (const s of slots) {
    for (const { r, c } of s.cells) {
      covered[r][c] = true;
    }
  }
  for (let r = 0; r < R; r++) {
    for (let c = 0; c < C; c++) {
      if (grid[r][c] === '.' && !covered[r][c]) {
        return { ok: false, why: 'uncovered' };
      }
    }
  }

  const slotCands = slots.map(s =>
    clueCandidates(grid, s).map(([r, c, arrow]) => ({
      def: defIndex.get(`${r},${c}`)!,
      cost: isStraightArrow(arrow) ? 0 : 1,
    }))
  );

  const assign = matchCluesMinCost(slots.length, defKeys.length, slotCands, 2);
  if (!assign) return { ok: false, why: 'match' };

  return { ok: true, slots, defKeys, assign };
}

export function buildCrossings(slots: Slot[]): { other: number; here: number; there: number }[][] {
  const cellMap = new Map<string, { slotIdx: number; pos: number }[]>();
  slots.forEach((s, si) => {
    s.cells.forEach(({ r, c }, pos) => {
      const key = `${r},${c}`;
      if (!cellMap.has(key)) cellMap.set(key, []);
      cellMap.get(key)!.push({ slotIdx: si, pos });
    });
  });

  const crossings: { other: number; here: number; there: number }[][] = slots.map(() => []);
  for (const refs of cellMap.values()) {
    if (refs.length < 2) continue;
    for (let i = 0; i < refs.length; i++) {
      for (let j = 0; j < refs.length; j++) {
        if (i === j) continue;
        crossings[refs[i].slotIdx].push({
          other: refs[j].slotIdx,
          here: refs[i].pos,
          there: refs[j].pos,
        });
      }
    }
  }
  return crossings;
}

export function solveFill(
  slots: Slot[],
  index: WordIndex,
  rng: () => number,
  nodeBudget = 50000
): { assigned: string[]; letters: (string | null)[][] } | null {
  const crossings = buildCrossings(slots);
  const assigned = new Array<string | null>(slots.length).fill(null);
  const letters = slots.map(s => new Array<string | null>(s.len).fill(null));
  const used = new Set<string>();
  let nodes = 0;

  const slotCandidates: string[][] = slots.map((s, si) => {
    return matchPattern(index, s.len, letters[si]);
  });

  const recurse = (): boolean => {
    if (++nodes > nodeBudget) return false;

    let target = -1;
    let minCandCount = Infinity;

    for (let si = 0; si < slots.length; si++) {
      if (assigned[si] !== null) continue;
      
      const len = slotCandidates[si].length;
      if (len === 0) return false;

      if (len < minCandCount) {
        minCandCount = len;
        target = si;
        if (len === 1) break;
      }
    }

    if (target === -1) return true;

    const rawList = slotCandidates[target];
    const bestList = rawList.filter(w => !used.has(w));
    if (bestList.length === 0) return false;

    shuffle(bestList, rng);
    const tryLimit = Math.min(bestList.length, 60);

    for (let t = 0; t < tryLimit; t++) {
      const word = bestList[t];
      const s = slots[target];

      const prevLetters = letters[target].slice();
      const touchedCrossings: { other: number; there: number; prevCands: string[] }[] = [];
      
      let consistent = true;
      assigned[target] = word;
      used.add(word);

      for (let i = 0; i < s.len; i++) {
        letters[target][i] = word[i];
      }

      for (let k = 0; k < crossings[target].length; k++) {
        const x = crossings[target][k];
        const character = word[x.here];
        const currentCrossingChar = letters[x.other][x.there];

        if (currentCrossingChar === null) {
          touchedCrossings.push({
            other: x.other,
            there: x.there,
            prevCands: slotCandidates[x.other]
          });

          letters[x.other][x.there] = character;
          
          const filtered = slotCandidates[x.other].filter(w => w[x.there] === character);
          slotCandidates[x.other] = filtered;

          if (filtered.length === 0) {
            consistent = false;
            break;
          }
        } else if (currentCrossingChar !== character) {
          consistent = false;
          break;
        }
      }

      if (consistent) {
        if (recurse()) return true;
      }

      assigned[target] = null;
      used.delete(word);
      letters[target] = prevLetters;
      
      for (let j = touchedCrossings.length - 1; j >= 0; j--) {
        const item = touchedCrossings[j];
        letters[item.other][item.there] = null;
        slotCandidates[item.other] = item.prevCands;
      }
    }
    return false;
  };

  const ok = recurse();
  return ok ? { assigned: assigned as string[], letters } : null;
}

export function arrowFor(s: Slot, dr: number, dc: number): string {
  if (s.dir === 'H') {
    if (dc === s.c - 1) return '→';
    if (dr === s.r - 1) return '↳';
    return '↱';
  }
  if (dr === s.r - 1) return '↓';
  if (dc === s.c - 1) return '⤵';
  return '⤓';
}

function assemble(
  grid: string[][],
  slots: Slot[],
  solution: { assigned: string[] },
  defKeys: { r: number; c: number }[],
  assign: number[]
): { grid: string[][]; defCells: Map<string, ArrowClue[]> } {
  const out = grid.map(row => row.slice());

  for (let si = 0; si < slots.length; si++) {
    const s = slots[si], word = solution.assigned[si];
    s.cells.forEach(({ r, c }, i) => {
      out[r][c] = word[i];
    });
  }

  const defCells = new Map<string, ArrowClue[]>();
  for (let si = 0; si < slots.length; si++) {
    const s = slots[si], word = solution.assigned[si];
    const { r: dr, c: dc } = defKeys[assign[si]];
    const key = `${dr},${dc}`;
    if (!defCells.has(key)) {
      defCells.set(key, []);
    }
    defCells.get(key)!.push({ word, dir: s.dir, arrow: arrowFor(s, dr, dc) });
  }

  return { grid: out, defCells };
}

export function generate(
  words: string[],
  R: number,
  C: number,
  opts: {
    templateAttempts?: number;
    nodeBudget?: number;
    seedString?: string;
  } = {}
): GenerationResult | null {
  const t0 = performance.now();
  const {
    templateAttempts = 500,
    nodeBudget = 50000
  } = opts;

  let seed = 0;
  let seedString = opts.seedString;
  if (seedString != null) {
    seed = hashSeedString(seedString);
  } else {
    seedString = randomSeedString();
    seed = hashSeedString(seedString);
  }

  const index = buildIndex(words);

  for (let a = 0; a < templateAttempts; a++) {
    const rng = makeRng((seed + a * 2654435761) >>> 0);
    const template = generateTemplate(R, C, rng);

    const analysis = analyzeTemplate(template, index);
    if (!analysis.ok || !analysis.slots || !analysis.defKeys || !analysis.assign) {
      continue;
    }

    const solution = solveFill(analysis.slots, index, rng, nodeBudget);
    if (!solution) {
      continue;
    }

    const { grid, defCells } = assemble(template, analysis.slots, solution, analysis.defKeys, analysis.assign);
    const generationTimeMs = Math.round(performance.now() - t0);

    return {
      grid,
      defCells,
      slots: analysis.slots,
      wordCount: analysis.slots.length,
      attempt: a,
      seed,
      seedString,
      generationTimeMs,
    };
  }
  return null;
}

export async function generateWithDefinitions(
  words: string[],
  R: number,
  C: number,
  opts: {
    templateAttempts?: number;
    nodeBudget?: number;
    seedString?: string;
  } = {}
): Promise<GenerationResult | null> {
  const result = generate(words, R, C, opts);
  if (!result) return null;

  const placed = new Set<string>();
  for (const clues of result.defCells.values()) {
    for (const clue of clues) placed.add(clue.word);
  }

  const byWord = await generateDefinitions(Array.from(placed));

  const definitions: Record<string, string> = {};
  for (const word of placed) {
    const def = byWord[word.toUpperCase()];
    if (def) definitions[word.toUpperCase()] = def;
  }

  result.definitions = definitions;
  return result;
}
