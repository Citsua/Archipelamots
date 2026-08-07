from __future__ import annotations

from typing import TYPE_CHECKING

from BaseClasses import ItemClassification, Location

from . import items
from .options import NbOfChecksPerGrid, TotalNbOfGrids

if TYPE_CHECKING:
    from .world import ArchipelamotsWorld

# Every location must have a unique integer ID associated with it.
# We will have a lookup from location name to ID here that, in world.py, we will import and bind to the world class.
# Even if a location doesn't exist on specific options, it must be present in this lookup.
def generate_locations() -> dict[str, int]:
    dictionary = {}
    id = 1
    for x in range(TotalNbOfGrids.range_end):
        for y in range(NbOfChecksPerGrid.range_end):
            dictionary[f"Complete Grid n°{str(x + 1)} ({str(y + 1)})"] = id
            id += 1
        for y in range(50):
            dictionary[f"Complete Word n°{str(y + 1)} in Grid n°{str(x + 1)}"] = id
            id += 1
    return dictionary

LOCATION_NAME_TO_ID = generate_locations()

# Each Location instance must correctly report the "game" it belongs to.
# To make this simple, it is common practice to subclass the basic Location class and override the "game" field.
class ArchipelamotsLocation(Location):
    game = "Archipelamots"

# Let's make one more helper method before we begin actually creating locations.
# Later on in the code, we'll want specific subsections of LOCATION_NAME_TO_ID.
# To reduce the chance of copy-paste errors writing something like {"Chest": LOCATION_NAME_TO_ID["Chest"]},
# let's make a helper method that takes a list of location names and returns them as a dict with their IDs.
# Note: There is a minor typing quirk here. Some functions want location addresses to be an "int | None",
# so while our function here only ever returns dict[str, int], we annotate it as dict[str, int | None].
def get_location_names_with_ids(location_names: list[str]) -> dict[str, int | None]:
    return {location_name: LOCATION_NAME_TO_ID[location_name] for location_name in location_names}


def create_all_locations(world: ArchipelamotsWorld) -> None:
    create_regular_locations(world)
    create_events(world)


def create_regular_locations(world: ArchipelamotsWorld) -> None:
    for x in range(world.options.total_nb_of_grids):
        region = world.get_region("Grid n°" + str(x + 1))
        for y in range(world.options.nb_of_checks_per_grid):
            region.add_locations(get_location_names_with_ids([f"Complete Grid n°{str(x + 1)} ({str(y + 1)})"]), ArchipelamotsLocation)
        for y in range(len(world.grids_data[x]["slots"])):
            region.add_locations(get_location_names_with_ids([f"Complete Word n°{str(y + 1)} in Grid n°{str(x + 1)}"]), ArchipelamotsLocation)


def create_events(world: ArchipelamotsWorld) -> None:
    for x in range(world.options.total_nb_of_grids):
        region = world.get_region("Grid n°" + str(x + 1))
        region.add_event(
            "Grid n°" + str(x + 1) + " Completed", "Complete Grid", location_type=ArchipelamotsLocation, item_type=items.ArchipelamotsItem
        )