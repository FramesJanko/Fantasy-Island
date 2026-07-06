# Netcode for Entities — Editor setup & verification (M0–M3)

This covers the **Editor-side wiring** for the first slice (packages → networked player movement).
All the C# is already in `Assets/Scripts/ECS/`. The old MonoBehaviour networking is untouched in
`Assets/Scripts/Old Networking/` (backups) and the live `Assets/Scripts/*.cs` (retired gradually —
do **not** put the old `PlayerControlledMovement` / `NPCControlledMovement` / `Health` / `Combat`
scripts on the new ghost prefabs).

## 0. Packages (done)

Your Multiplayer Center already installed the stack: `com.unity.netcode` 1.14.0 (Netcode for Entities),
`com.unity.entities.graphics` 6.4.0 (pulls `com.unity.entities`), `com.unity.services.multiplayer` (for the
M8 Lobby+Relay phase), and the PlayMode tools. Let Unity finish compiling and confirm the **Console has no
errors** from `FantasyIsland.Gameplay` before continuing.

## 1. Create a SubScene

1. In `Assets/Scenes/SampleScene.unity`, right-click in the Hierarchy ▸ **New Sub Scene ▸ Empty Scene**.
   Name it `Gameplay`. (This is what bakes GameObjects into entities for both the client and server worlds.)

## 2. Build the Player ghost prefab

1. Create a prefab `Assets/Prefabs/PlayerUnit.prefab` (e.g. a Capsule so it renders via Entities Graphics —
   it needs a **MeshRenderer + MeshFilter** with a **URP material**).
2. Add these components (from `Assets/Scripts/ECS/Authoring/`):
   - **UnitAuthoring** (set maxHealth, moveSpeed, attack fields)
   - **PlayerAuthoring**
   - **GhostAuthoringComponent** (built-in, from the netcode package). Configure:
     - **Default Ghost Mode:** `Interpolated`
     - **Has Owner:** ✅ (this adds the `GhostOwner` the server stamps with the client's NetworkId)
     - To *see/verify* what replicates, add the optional **Ghost Authoring Inspection Component** (Add Component
       ▸ it lives next to GhostAuthoring). It lists every replicated component. Confirm **LocalTransform** is
       present with its default variant (e.g. `Transform - 3D`) — that's what carries movement to clients — and
       that **Health** appears (via its `[GhostField]`). You don't need to change anything; just confirm they're
       there and not set to a "Don't Serialize" variant. The inspection component can be removed afterward.
3. **Remove** any old MonoBehaviours (`PlayerControlledMovement`, `UnitInfo`, `Health`, `Combat`, `NavMeshAgent`)
   from this prefab — the ECS systems own that behaviour now.

## 3. Build the NPC ghost prefab

1. Create `Assets/Prefabs/NpcUnit.prefab` (another rendered mesh).
2. Add: **UnitAuthoring**, **NpcAuthoring** (set huntingDistance / leashRange), **GhostAuthoringComponent**
   (`Interpolated`, **Has Owner: off**), LocalTransform Position replicated.
3. Remove old MonoBehaviours as above.

## 4. Add the spawner to the SubScene

1. Inside the `Gameplay` SubScene, create an empty GameObject `GameSpawner`.
2. Add **GameSpawnerAuthoring** and assign:
   - **Player Prefab** → `PlayerUnit.prefab`
   - **Npc Prefab** → `NpcUnit.prefab`
   - **Npc Spawn Points** → drop in a few empty transforms placed around the arena (one NPC spawns per point).

## 5. PlayMode Tools

1. **Window ▸ Multiplayer ▸ PlayMode Tools**. Set **PlayMode Type = Client & Server**, **Num Thin Clients = 0**
   to start (1 real client + server in one Editor).
2. The bootstrap (`GameBootstrap`) auto-connects to `127.0.0.1:7979`, so pressing **Play** connects automatically.

## Verification

**M2 — spawn**
- Press Play. In the **Entities Hierarchy** (Window ▸ Entities ▸ Hierarchy), select the **server** world:
  you should see one `PlayerUnit` per connection plus one `NpcUnit` per spawn point. Select the **client**
  world: the same ghosts should be mirrored. The player/NPC meshes render in the Game view.

**M3 — movement**
- **Right-click** on the ground. The RPC (`MoveCommandRpc`) travels to the server, `MoveCommandReceiveSystem`
  sets your unit's `MoveDestination`, and `MovementSystem` steers it there. The unit should glide to the
  clicked point. Add a **thin/second client** (PlayMode Tools ▸ Num Thin Clients = 1, or a ParrelSync/second
  editor) to confirm the movement replicates to the other peer.
- To watch it live: select the player entity in the server world and observe `LocalTransform.Position`
  changing, and `MoveDestination` toggling enabled → disabled on arrival.

## Troubleshooting

- **Nothing spawns:** confirm the SubScene is open/loaded and `GameSpawner` has both prefabs assigned; check the
  server world actually has the `PlayerSpawner`/`NpcSpawner` singletons (Entities Hierarchy search).
- **Player spawns but doesn't move:** verify **Has Owner** is on for the Player prefab (no owner ⇒ the server
  can't map your NetworkId to a unit). Confirm right-click hits a collider (the ground needs one for the raycast).
- **Moves on server but not on client:** LocalTransform Position isn't being replicated — re-check the
  GhostAuthoring component list.
- **`GameBootstrap` not used / connection errors:** ensure only one `ClientServerBootstrap` subclass exists and
  the Console shows the client connecting on 7979.

## What's next

- **M4:** bring your A* Grid/Node/Pathfinding classes into the project; I'll swap `PathRequestSystem`'s single
  waypoint for a real route (movement/AI unchanged).
- **M5–M7:** NPC AI (follow/leash), combat + health, and the UI bridge.
- **M8:** replace the localhost auto-connect with UGS **Relay** and rebuild the lobby name/list/join on UGS **Lobby**.
