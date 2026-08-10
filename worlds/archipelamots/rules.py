from __future__ import annotations

from typing import TYPE_CHECKING

from rule_builder.options import OptionFilter
from rule_builder.rules import Has, HasAll, Rule

if TYPE_CHECKING:
    from .world import ArchipelamotsWorld

def set_all_rules(world: ArchipelamotsWorld) -> None:
    # In order for AP to generate an item layout that is actually possible for the player to complete,
    # we need to define rules for our Entrances and Locations.
    # Note: Regions do not have rules, the Entrances connecting them do!
    # We'll do entrances first, then locations, and then finally we set our victory condition.

    set_all_entrance_rules(world)
    set_all_location_rules(world)
    set_completion_condition(world)


def set_all_entrance_rules(world: ArchipelamotsWorld) -> None:
    pass

def set_all_location_rules(world: ArchipelamotsWorld) -> None:
    for x in range(world.options.total_nb_of_grids):
        has_grid = Has(f"Grid n°{str(x + 1)}")
        complete_grid_requirement = has_grid
        for y in range(len(world.grids_data[x]["slots"])):
            location = world.get_location(f"Complete Word n°{str(y + 1)} in Grid n°{str(x + 1)}")
            is_pre_revealed = world.grids_data[x]["definitions"][y]["revealed"]
            if is_pre_revealed:
                world.set_rule(location, has_grid)
            else:
                has_definition = Has(f"Definition n°{str(y + 1)} from Grid n°{str(x + 1)}")
                complete_grid_requirement = complete_grid_requirement & has_definition
                has_grid_and_definition = has_grid & has_definition
                world.set_rule(location, has_grid_and_definition)
        for y in range(world.options.nb_of_checks_per_grid):
            location = world.get_location(f"Complete Grid n°{str(x + 1)} ({str(y + 1)})")
            world.set_rule(location, complete_grid_requirement)


def set_completion_condition(world: ArchipelamotsWorld) -> None:
    # In our case, we went for the Victory event design pattern (see create_events() in locations.py).
    # So lets undo what we just did, and instead set the completion condition to:
    completed_grids_needed = round((world.options.percentage_of_grids_to_win / 100.0) * world.options.total_nb_of_grids)
    world.set_completion_rule(Has("Complete Grid", completed_grids_needed))