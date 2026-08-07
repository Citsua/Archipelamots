from __future__ import annotations


import typing
from dataclasses import dataclass
import typing
from Options import Choice, OptionGroup, FreeText, PerGameCommonOptions, Range, Toggle, Visibility, Option

# In this file, we define the options the player can pick.
# The most common types of options are Toggle, Range and Choice.

# Options will be in the game's template yaml.
# They will be represented by checkboxes, sliders etc. on the game's options page on the website.
# (Note: Options can also be made invisible from either of these places by overriding Option.visibility.
#  APQuest doesn't have an example of this, but this can be used for secret / hidden / advanced options.)

# For further reading on options, you can also read the Options API Document:
# https://github.com/ArchipelagoMW/Archipelago/blob/main/docs/options%20api.md


class TotalNbOfGrids(Range):

    display_name = "Total number of grids"

    range_start = 0
    range_end = 100
    default = 20
    
class PercentageOfGridsToWin(Range):

    display_name = "Percentage of grids that need to be completed"

    range_start = 0
    range_end = 100
    default = 80
    
class NbOfChecksPerGrid(Range):

    display_name = "Number of checks unlocked when completing a grid"

    range_start = 0
    range_end = 10
    default = 3
    
class NbOfStartingGrids(Range):
    
    display_name = "Number of grids unlocked at start"

    range_start = 0
    range_end = 100
    default = 3

class PercentageOfDefinitionsUnlockedAtStart(Range):
    display_name = "Percentage of definitions unlocked at the start of a grid"

    range_start = 0
    range_end = 100
    default = 30

class GridData(FreeText):
    Visibility = Visibility.none

# We must now define a dataclass inheriting from PerGameCommonOptions that we put all our options in.
# This is in the format "option_name_in_snake_case: OptionClassName".
@dataclass
class ArchipelamotsOptions(PerGameCommonOptions):
    total_nb_of_grids: TotalNbOfGrids
    percentage_of_grids_to_win: PercentageOfGridsToWin
    nb_of_checks_per_grid: NbOfChecksPerGrid
    nb_of_starting_grids: NbOfStartingGrids
    percentage_of_definitions_unlocked_at_start: PercentageOfDefinitionsUnlockedAtStart
    grid_data: GridData