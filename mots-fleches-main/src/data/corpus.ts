import fs from 'fs';

function stripAccents(s: string): string {
  return s.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
}

const acronyms = [
  'ADN', 'ARN',  'ASBL', 'ATAF', 'BBC',  'BTP',  'BDD',  'BLOB', 'BTS',
  'CAP', 'CDD',  'CDI',  'CERN', 'CNIL', 'CNRS', 'COUR', 'DAB',  'DAM',
  'DAS', 'DEFM', 'DRH',  'DVD',  'EDF',  'EFT',  'EMI',  'ESS',  'ETA',
  'FAO', 'FBI',  'FFTL', 'FMI',  'FNAC', 'FOAN', 'GAZ',  'GIGN', 'GRH', 
  'GSM', 'HAS',  'HQE',  'HTML', 'HTTP', 'IADC', 'IAS',  'IBM',  'ICT', 
  'IDE', 'IFS',  'IGH',  'IMD',  'IMI',  'INA',  'JAA',  'JAR',  'JAT', 
  'JDA', 'KPI',  'KTV',  'LAM',  'LDS',  'LGV',  'LSF',  'MAE',  'MCD', 
  'MDD', 'MEA',  'MNT',  'NAD',  'NBA',  'NCF',  'NFC',  'NFS',  'NGO', 
  'NIF', 'OAT',  'OCP',  'ODD',  'OEM',  'OGM',  'OKR',  'OMS',  'ONG', 
  'ONU', 'OPA',  'OPE',  'PAC',  'PAD',  'PAN',  'PAO',  'PAS',  'PCC', 
  'PCE', 'PDA',  'PDF',  'PDG',  'PEA',  'PEC',  'QAI',  'QDA',  'QED',
  'RAD', 'RAF',  'RAI',  'RAM',  'RAS',  'RDV',  'REA',  'REC',  'REP', 
  'RES', 'RFC',  'RFI',  'SAE',  'SAI',  'SAR',  'SAS',  'SAT',  'SCA', 
  'SCI', 'SDS',  'SEA',  'SFER', 'SIG',  'SIR',  'TAB',  'TAC',  'TAE', 
  'TAO', 'TAR',  'TAS',  'TAT',  'TAU',  'TAV',  'TAX',  'TBC',  'TCH', 
  'UER', 'UFA',  'UGC',  'UGT',  'UIT',  'ULD',  'UML',  'UNA',  'UNI', 
  'UPE', 'URA',  'URI',  'VAE',  'VAN',  'VAR',  'VAS',  'VAT',  'VCA', 
  'VDI', 'VEC',  'VER',  'VES',  'WAB',  'WAN',  'WAR',  'WAS',  'WCD', 
  'WEB', 'WEC',  'WED',  'WER',  'WES',  'XAO',  'XAS',  'XML',  'XPS',
  'YAK', 'YAM',  'YAN',  'YAR',  'YAS',  'YEA',  'YEP',  'YES',  'ZAC', 
  'ZAG', 'ZAP',  'ZAR',  'ZAS',  'ZED',  'ZEE',  'ZEK',  'ZEN',  'ZEP', 
  'ZER', 'ZES',  'USA',  'DVD',  'GPS',  'SMS',  'USB',  'VTT',  'VAE', 
  'CDI', 'CDD',  'RDV'
];

const data = fs.readFileSync('Lexique383.tsv', 'utf-8');
const lines = data.split('\n');
const headers = lines[0].split('\t');

const idx = {
  ortho: headers.indexOf('ortho'),
  lemme: headers.indexOf('lemme'),
  cgram: headers.indexOf('cgram'),
  freqfilms2: headers.indexOf('freqfilms2'),
  freqlivres: headers.indexOf('freqlivres'),
  nblettres: headers.indexOf('nblettres'),
};

const seen = new Map();

for (const line of lines.slice(1)) {
  const f = line.split('\t');
  if (f.length < headers.length) continue;

  const ortho = f[idx.ortho];
  const lemme = f[idx.lemme];
  const cgram = f[idx.cgram];
  const freqfilms2 = parseFloat(f[idx.freqfilms2]) || 0;
  const freqlivres = parseFloat(f[idx.freqlivres]) || 0;
  const nblettres = parseInt(f[idx.nblettres]) || 0;

  if (!['NOM', 'ADJ', 'ADV', 'VER'].includes(cgram)) continue;
  if (cgram === 'VER' && ortho !== lemme) continue;

  if (nblettres < 2 || nblettres > 11) continue;
  if (/[\s'\-]/.test(ortho)) continue;

  const freqThreshold = nblettres <= 2 ? 0.02 : nblettres <= 4 ? 0.1 : 0.5;
  if (freqfilms2 < freqThreshold) continue;

  const ratioThreshold = nblettres <= 2 ? 100 : nblettres <= 4 ? 30 : 15;
  if (freqlivres / (freqfilms2 + 0.1) > ratioThreshold) continue;

  const word = stripAccents(ortho).toUpperCase();
  if (!/^[A-Z]+$/.test(word)) continue;
  if (word.length !== nblettres) continue;

  const prev = seen.get(word);
  if (!prev || prev.freqfilms2 < freqfilms2) {
    seen.set(word, { word, freqfilms2 });
  }
}

for (const acronym of acronyms) {
  if (acronym.length < 2 || acronym.length > 4) continue;
  if (!seen.has(acronym)) {
    seen.set(acronym, { word: acronym, freqfilms2: 0.001 });
  }
}

const words = [...seen.values()].sort((a, b) => b.freqfilms2 - a.freqfilms2);
const n = words.length;
const t1 = Math.floor(n * 0.30);
const t2 = Math.floor(n * 0.70);

export const corpus = {
  easy:   words.slice(0,  t1).map(w => w.word),
  medium: words.slice(t1, t2).map(w => w.word),
  hard:   words.slice(t2).map(w => w.word),
};

fs.writeFileSync('corpus.json', JSON.stringify(corpus));
console.log(`easy: ${corpus.easy.length}, medium: ${corpus.medium.length}, hard: ${corpus.hard.length}`);
console.log(`Acronymes ajoutés: ${acronyms.filter(a => seen.has(a)).length}`);