import { generate, getAllWords } from 'mots-fleches'
import { load } from 'js-yaml'
import { readFileSync } from 'node:fs'
import { readdirSync } from 'node:fs'
import * as cheerio from 'cheerio'
import axios from 'axios'

const apikey = '848a1f75297c3fa2f07b672692e059f21eac38a2';
var folderPath = process.cwd();

// find YAML file
var files = readdirSync(`${folderPath}\\yaml`).filter(fn => fn.endsWith('.yaml'));
if (files.length != 1) {
  throw "There should be exactly one YAML file in the folder.";
}

// read YAML file
var filePath = `${folderPath}\\yaml\\${files[0]}`;
console.log(`yaml file: ${filePath}`);
const doc = load(readFileSync(filePath, 'utf8'));
console.log(doc);

var grids = [];
// generate grids
for (var i = 0; i < doc.Archipelamots.total_nb_of_grids; i++) {
  console.log(`generating grid ${i + 1}`);
  var correct;
  do {
    correct = true;
    console.log(`trying to generate`);
    var result = generate(getAllWords(), 6, 6);
    result.definitions = [];
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
        result.definitions.push({ word: word, definition: randomDefinition });
        console.log(`${word}: ${randomDefinition}`);
      } catch (error) {
        console.error(error);
        correct = false;
        break;
      }
    }

    if (correct) {
      grids.push(result);
      console.log(result);
    }

  } while (!correct);
}

console.log("generated all grids")

// add the grids into the yaml
// TODO

function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

// read and parse definitions database
/*var databaseString = readFileSync(`${folderPath}\\definitions.tsv`, 'utf8');
var database = [];

var x = databaseString.split('\n');
for (var i = 0; i < x.length; i++) {
  var y = x[i].split('\t');
  database.push({
      word:   y[0],
      definitions: y.slice(1, y.length)
  });
}
console.log(database);*/

/*var databaseEquivalent = database.find(x => x.word == word);
if (databaseEquivalent != undefined) {
  result.definitions.push({ word: word, definition: databaseEquivalent[Math.floor(Math.random() * databaseEquivalent.length)] })
}
else {
  console.log(`word '${word}' wasn't found in the definitions database. replacing the grid.`);
  correct = false;
  break;
}*/