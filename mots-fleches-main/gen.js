import { generate, getAllWords, getEasyWords, getEasyAndMediumWords } from 'mots-fleches'
import { load, dump } from 'js-yaml'
import { readFileSync, readdirSync, writeFileSync } from 'node:fs'
import * as cheerio from 'cheerio'
import axios from 'axios'

import * as util from 'util'

const apikey = '848a1f75297c3fa2f07b672692e059f21eac38a2';
var folderPath = process.cwd();

// find YAML file
var files = readdirSync(`${folderPath}\\yaml`).filter(fn => fn.endsWith('.yaml') && !fn.includes('-generated'));
if (files.length != 1) {
  throw "There should be exactly one YAML file in the folder.";
}

// read YAML file
var filePath = `${folderPath}\\yaml\\${files[0]}`;
console.log(`yaml file: ${filePath}`);
var doc = load(readFileSync(filePath, 'utf8'));
console.log(doc);

// grid sizes
var easy = [7, 7]
var medium = [7, 10]
var hard = [10, 10]

// generate grids
var grids = [];
for (var i = 0; i < doc.Archipelamots.total_nb_of_grids; i++) {
  console.log(`generating grid ${i + 1}`);

  var difficulty = i / (doc.Archipelamots.total_nb_of_grids - 1);
  var gridSize = []
  var words = []
  if (difficulty < 0.34) {
    gridSize = easy;
    words = getEasyWords()
  }
  else if (difficulty < 0.67) {
    gridSize = medium;
    words = getEasyAndMediumWords()
  }
  else {
    gridSize = hard;
    words = getAllWords()
  }

 console.log("difficulty: " + difficulty);
 console.log("nb of possible words: " + words.length);
 console.log(`size: (${gridSize[0]}, ${gridSize[1]})`);

  var correct;
  do {
    correct = true;

    console.log(`trying to generate`);
    var result = null;
    while (result == null) {
      result = generate(words, gridSize[0], gridSize[1]);
    }
    console.log(result);

    var defCellsArray = [];
    for (let [key, value] of result.defCells.entries()) {
      var coords = key.split(',');
      var defCell = { coords: {r: parseInt(coords[0]), c: parseInt(coords[1])}, definitions: value };
      defCellsArray.push(defCell);
    }

    var preRevealedDefinitions = getRandomIds(range(result.slots.length), Math.round(result.slots.length * (doc.Archipelamots.percentage_of_definitions_unlocked_at_start / 100.0)));

    result.definitions = [];
    result.defCells = defCellsArray;
    for (var j = 0; j < result.slots.length; j++) {
      var element = result.slots[j];
      var word = "";
      for (var k = 0; k < element.cells.length; k++) {
        word += result.grid[element.cells[k].r][element.cells[k].c];
      }

      try {
        var axiosResponse = await axios.request({
          method: "GET",
          url: 'https://api.zenrows.com/v1/',
          params: {
            'url': `https://www.fsolver.fr/mots-fleches/${word}`,
            'apikey': apikey,
            'mode': 'auto',
          },
        });

        const $ = cheerio.load(axiosResponse.data);
        const definitions = $("div#definitions").find("span[itemprop='text']").map(function () {
          return $(this).text().trim()
        }).toArray();

        if (definitions.length < 1) {
          throw "No definition found for " + word;
        }

        var randomDefinition = definitions[getRandomInt(0, definitions.length - 1)];
        result.definitions.push({ word: word, definition: randomDefinition, revealed: preRevealedDefinitions.includes(result.definitions.length) });
        console.log(`${word}: ${randomDefinition}`);
      } catch (error) {
        console.error(error);
        correct = false;
        break;
      }
    }

    if (correct) {
      grids.push(result);
    }

  } while (!correct);
}

console.log("generated all grids")

// add the grids into the yaml
doc.Archipelamots["grid_data"] = dump(grids);
console.log(util.inspect(doc, {showHidden: false, depth: null, colors: true}))
var newFilePath = `${filePath.substring(0, filePath.indexOf('.yaml'))}-generated.yaml`;
writeFileSync(newFilePath, dump(doc), (err) => {
    if (err) {
        console.log(err);
    }
});


function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function range(start, end, step = 1) {
  let output = [];

  if (typeof end === 'undefined') {
    end = start;
    start = 0;
  }

  for (let i = start; i < end; i += step) {
    output.push(i);
  }

  return output;
}

function getRandomIds(array, count) {
  var scrambled = array.sort(() => Math.random() - 0.5);
  return scrambled.slice(0, count);
}