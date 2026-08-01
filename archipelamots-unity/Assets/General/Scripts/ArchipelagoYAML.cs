using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArchipelagoYAML
{
    public string name;
    public string description;
    public string game;
    public Archipelamots Archipelamots;
}

public class Archipelamots
{
    public string progression_balancing;
    public string accessibility;
    public int total_nb_of_grids;
    public int percentage_of_grids_to_win;
    public int nb_of_checks_per_grid;
    public int nb_of_starting_grids;

    public string[] local_items;
    public string[] non_local_items;
    public Dictionary<string, string> start_inventory;
    public string[] start_hints;
    public string[] start_location_hints;
    public string[] exclude_locations;
    public string[] priority_locations;
    public Grid[] grids;

    public class Grid
    {
        public char[][] grid;
        public DefCell[] defCells;
        public Slot[] slots;
        public int wordCount;
        public int attempt;
        public long seed;
        public string seedString;
        public long generationTimeMs;
        public Definition[] definitions;

        public string GetDefinition(string word)
        {
            return this.definitions.First(x => x.word == word).definition;
        }
    }

    public class DefCell
    {
        public Cell coords;
        public DefCellInfo[] definitions;
    }

    public class DefCellInfo
    {
        public string word;
        public char dir;
        public char arrow;
    }

    public class Definition
    {
        public string word;
        public string definition;
    }

    public class Slot
    {
        public int r;
        public int c;
        public string dir;
        public int len;
        public Cell[] cells;
    }

    public struct Cell
    {
        public int r;
        public int c;
    }
}