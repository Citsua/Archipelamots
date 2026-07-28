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
    pass

def set_completion_condition(world: ArchipelamotsWorld) -> None:
    # In our case, we went for the Victory event design pattern (see create_events() in locations.py).
    # So lets undo what we just did, and instead set the completion condition to:
    completed_grids_needed = round((world.options.percentage_of_grids_to_win / 100.0) * world.options.total_nb_of_grids)
    world.set_completion_rule(Has("Complete Grid", completed_grids_needed))