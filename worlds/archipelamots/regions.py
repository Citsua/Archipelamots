from __future__ import annotations

from typing import TYPE_CHECKING

from BaseClasses import Entrance, Region

import os

if TYPE_CHECKING:
    from .world import ArchipelamotsWorld

# A region is a container for locations ("checks"), which connects to other regions via "Entrance" objects.
# Many games will model their Regions after physical in-game places, but you can also have more abstract regions.
# For a location to be in logic, its containing region must be reachable.
# The Entrances connecting regions can have rules - more on that in rules.py.
# This makes regions especially useful for traversal logic ("Can the player reach this part of the map?")

# Every location must be inside a region, and you must have at least one region.
# This is why we create regions first, and then later we create the locations (in locations.py).


def create_and_connect_regions(world: ArchipelamotsWorld) -> None:
    create_all_regions(world)
    connect_regions(world)


def create_all_regions(world: ArchipelamotsWorld) -> None:
    # Creating a region is as simple as calling the constructor of the Region class.
    
    regions = [Region(world.origin_region_name, world.player, world.multiworld)]
    for x in range(world.options.total_nb_of_grids):
        region = Region("Grid n°" + str(x + 1), world.player, world.multiworld)
        regions.append(region)

    # We now need to add these regions to multiworld.regions so that AP knows about their existence.
    world.multiworld.regions += regions


def connect_regions(world: ArchipelamotsWorld) -> None:
    menu = world.get_region(world.origin_region_name)
    
    for x in range(world.options.total_nb_of_grids):
        region = world.get_region("Grid n°" + str(x + 1))
        menu.connect(region, "Menu to Grid n°" + str(x + 1), lambda state: state.has("Grid n°" + str(x + 1), world.player))
