# ItemManager

SCP: Secret Laboratory **LabAPI 1.1.7** plugin with three features:

1. **LCZ Cleaner** — after 20 minutes of round time (LCZ is decontaminated anyway), every leftover
   item pickup and dead body in the Light Containment Zone is deleted, and the zone keeps being
   swept every 60 seconds so the server stops simulating junk nobody can reach.
2. **Death Item Mover** — when a player dies, everything they dropped (items, ammo, armor) is
   teleported to the Surface near the A elevators at `(0.007, 300.96, 0.125)`, scattered randomly
   in a circle and dropped ~1 m above the floor so physics settles each piece
   on the ground.
3. **Glowing Item Spawner** — RemoteAdmin command that spawns items scattered on the floor of the
   room you are in, each with a colored glow light matching the item (green medkit, cyan
   adrenaline, red SCP-500, yellow scientist card, ...). The glow follows the item and disappears
   when it is picked up or deleted.
4. **Auto Spawner** — a few seconds after every round start, glowing items are automatically
   scattered on the floor of random rooms, driven by `auto_spawn_rules` (zone, optional room
   list, item pool, count per round). Defaults spawn medical items, keycards, guns with ammo,
   armor and rare SCP items across LCZ/HCZ/Entrance, tiered by zone.


## Installing

Copy `ItemManager.dll` to the server's LabAPI plugin folder:

- Windows: `%AppData%\SCP Secret Laboratory\LabAPI\plugins\<port or global>\`
- Linux: `~/.config/SCP Secret Laboratory/LabAPI/plugins/<port or global>/`

The config is generated on first start at `LabAPI\configs\<port or global>\ItemManager\config.yml`.


## Config highlights

| Key                        | Default                  | Meaning                                                                       |
|----------------------------|--------------------------|-------------------------------------------------------------------------------|
| `cleaner_start_minutes`    | `20`                     | round time before LCZ cleanup activates                                       |
| `cleaner_interval_seconds` | `60`                     | sweep frequency once active                                                   |
| `cleaner_remove_ragdolls`  | `true`                   | also delete corpses in LCZ                                                    |
| `cleaner_all_lcz_items`    | `true`                   | delete everything in LCZ; set `false` to only delete `cleaner_low_tier_items` |
| `mover_target_x/y/z`       | `0.007 / 300.96 / 0.125` | surface drop zone near A elevators                                            |
| `mover_scatter_radius`     | `3.5`                    | scatter circle radius for moved death drops                                   |
| `spawner_glow_enabled`     | `true`                   | glow lights on spawned items                                                  |
| `auto_spawner_enabled`     | `true`                   | automatic glowing item spawns at round start                                  |
| `auto_spawn_delay_seconds` | `5`                      | wait after round start before auto spawns                                     |
| `auto_spawn_rules`         | LCZ/HCZ/EZ medical       | list of rules: `zone`, `rooms` (empty = any), `items`, `count`, `glow`        |
| `glow_colors`              | per-item map             | HTML hex color per ItemType; category-based fallback otherwise                |
