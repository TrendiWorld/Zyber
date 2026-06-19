# Zyber

A *Little Fighters 2*–style local co-op brawler (Unity). A roster of role-based
fighters — including the **Ninja** assassin with custom art + procedural
animation — plus test enemies. Gameplay is built in C#, rendered with runtime
placeholder art where final assets aren't in yet.

## For collaborators (levels & enemies)

- **Enemies** live in `Assets/Scripts/Data/Roster.cs` (`Enemies()`) with AI in
  `Assets/Scripts/Runtime/EnemyController.cs` — add an `EnemyDef` + an `EnemyAi`
  branch to create a new enemy type.
- **Spawning / arena layout** is in `Assets/Scripts/Runtime/GameBootstrap.cs`
  (`BuildArena` / `SpawnAll`) and `Arena.cs` — this is where level/wave setup
  would hook in.
- Everything auto-boots on Play via `GameBootstrap` (no scene wiring needed yet);
  a proper scene/level system is a natural next addition.

## Run it

1. **Install an Editor** (one-time): open **Unity Hub → Installs → Install Editor**
   and pick **Unity 6 LTS** (any `6000.0.x` LTS build). Default modules are fine —
   you don't need any platform build support just to play in the editor.
2. **Add this project**: Unity Hub → **Projects → Add → Add project from disk** →
   select this folder (`/Users/admin/lf2-unity`). Open it.
   - If Hub warns the editor version differs, choose your installed 6 LTS and
     continue — it will adopt the project.
3. **Press Play.** That's it. The arena, two players, three enemies, and the HUD
   are all built at runtime (`GameBootstrap` auto-boots via
   `[RuntimeInitializeOnLoadMethod]`), so there is no scene to wire up and no art
   to import — everything renders as colored boxes, just like the prototype.

## Controls (local co-op, 2 players)

| Action  | Player 1 | Player 2 |
|---------|----------|----------|
| Move    | A / D    | ← / →    |
| Jump    | W        | ↑        |
| Attack  | F        | .        |
| Ability 1 | G      | ,        |
| Ability 2 | H      | M        |

- **R** — reset the arena
- **[ ` / ` ]** — cycle Player 1's character through the roster

## Where things live

| File | Purpose |
|------|---------|
| `Assets/Scripts/Data/Roster.cs` | **The roster** — characters, abilities, enemies (edit here) |
| `Assets/Scripts/Data/AbilityDef.cs` | All ability tuning fields |
| `Assets/Scripts/Runtime/Fighter.cs` | Combat body: hp/energy, movement, knockback, buffs |
| `Assets/Scripts/Runtime/AbilitySystem.cs` | One branch per ability type (the executeAbility port) |
| `Assets/Scripts/Runtime/EnemyController.cs` | Test-enemy AI (dummy / chaser / ranged) |
| `Assets/Scripts/Runtime/GameBootstrap.cs` | Builds the arena & spawns everything at Play |

## What's faithful to the prototype

All gameplay numbers (damage, cooldowns, energy, speeds) are in the **same units**
as the JS version — the sim runs a fixed 60 fps step (`Time.fixedDeltaTime = 1/60`)
so "per-frame" values port 1:1.

## Next steps (not done yet)

- Real sprite sheets + animation clips (currently colored boxes; the `clip`
  field on each ability is preserved for when art lands)
- 2.5D depth (up/down movement on the floor) — currently pure side-view
- New Input System + gamepads for >2 players
- Character-select screen (roster data is already there to drive it)
