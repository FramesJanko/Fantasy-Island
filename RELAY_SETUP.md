# M8 — Cross-network play via UGS Relay + Lobby

This is the setup for the code added under `Assets/Scripts/Networking/`. It replaces the
localhost auto-connect so two players **on different home networks** connect to each other —
no port forwarding — by tunnelling Netcode-for-Entities traffic through **Unity Relay**, with
a live **lobby browser** backed by **Unity Lobby**.

**Model:** one player **Hosts** (their process runs the Netcode *server* world + a local
*client* world). Everyone else **Joins** as a pure client. The Relay join code is stored in the
lobby data, so joiners click a lobby in the browser and never type a code.

---

## What changed in code

| File | Role |
|---|---|
| `ECS/Bootstrap/GameBootstrap.cs` | No longer auto-connects. Creates only the local world at startup so the menu can run; the server/client worlds are created on demand. |
| `Networking/RelayDriverConstructor.cs` | Feeds Relay data into Netcode's transport driver (`INetworkStreamDriverConstructor`). |
| `Networking/RelayConnectionManager.cs` | The orchestrator: UGS init + anonymous sign-in, host (allocate + create lobby + heartbeat), browse, join, and Netcode world creation. |
| `Networking/RelayLobbyUI.cs` | Minimal lobby-browser UI (host field, refresh, list of joinable lobbies, status). |

The old MonoBehaviour `NetworkManager` / `LobbyMenu` / `HostButton` flow is untouched but is now
superseded — see step 5.

---

## 1. Dashboard / project linking (one-time, required)

Relay and Lobby are cloud services, so the project must be linked to a Unity Gaming Services
project and the services enabled.

1. **Edit ▸ Project Settings ▸ Services** — sign in and **link** the project to a Unity org /
   cloud project (or create one). This writes your Project ID into `ProjectSettings`.
2. Open the **Unity Cloud dashboard** (`cloud.unity.com`) for that project and enable:
   - **Relay** (has a free tier — bandwidth/CCU metered)
   - **Lobby** (free)
   - **Authentication** — no config needed; we use **Anonymous** sign-in, enabled by default.
3. No API keys go in the client — the SDK authenticates via the linked project + anonymous auth.

> If hosting throws `Unauthorized` / `project not linked`, this step is incomplete.

## 2. Package check (already done)

`com.unity.services.multiplayer` 2.2.4 (bundles Relay + Lobby), `com.unity.services.authentication`,
and `com.unity.netcode` 1.14 are all installed. Let Unity compile the new
`FantasyIsland.Networking` assembly and confirm **no Console errors**.

## 3. Scene wiring

1. In `SampleScene`, create an empty GameObject **`RelayConnectionManager`** and add the
   **RelayConnectionManager** component. (It's a `DontDestroyOnLoad` singleton — one is enough.)
   - `Max Players` = **2** (host + 1 joiner). `Relay Connection Type` = **dtls** (leave as-is;
     use `wss` only for a WebGL build).
   - **`Gameplay Sub Scenes`** = drag your **Gameplay** SubScene GameObject here (the one holding
     `GameSpawner` + spawn points). **Required** — the server/client worlds are created only when
     you Host/Join, which is *after* the SubScene's own auto-load has run, so they'd otherwise
     never receive it and nothing would spawn. This field streams it into each netcode world.
     Set the SubScene's **Auto Load Scene** as you like; it doesn't affect this path.
2. Build a simple menu Canvas with:
   - a **TMP_InputField** for the lobby name,
   - a **Host** button,
   - a **Refresh** button,
   - a **scroll view / vertical-layout container** for the lobby list,
   - a **lobby button prefab**: a `Button` with a child `TMP_Text` (set it **inactive** in the
     project; the UI clones it per lobby),
   - a **status** `TMP_Text`.
3. Add the **RelayLobbyUI** component (to the Canvas or a child) and drag each of the above into
   its fields: `Manager` → the RelayConnectionManager, `Lobby Name Input`, `Host Button`,
   `Refresh Button`, `Lobby List Content` (the container transform), `Lobby Button Prefab`,
   `Status Text`, and optionally `Menu Root` (the panel to hide once a session starts).

## 4. Play flow

- **Host:** type a name → **Host**. Status shows `Hosting "<name>" (join code …)`; a lobby is
  created and heartbeated every 15 s so it stays listed.
- **Join:** **Refresh** → the list fills with open lobbies → click one. The client joins the
  lobby, reads the Relay join code, and connects through Relay. `AutoInGameSystem` flips the
  connection in-game and your existing ghost spawning/movement takes over unchanged.

## 5. Retire the old menu

The legacy `NetworkManager` (custom TCP/UDP relay), `LobbyMenu`, and `HostButton` target a
separate self-hosted server and are **not** used by this path. Disable/remove those GameObjects
in the scene so only the new RelayConnectionManager + RelayLobbyUI drive connections. (Leave the
`.cs` files for now if other scene objects still reference them.)

## 6. Testing across networks

- **Same machine (quickest smoke test):** Relay works even locally — Host in one editor/build and
  Join in a second (use **ParrelSync**, a second clone, or a standalone build alongside the
  editor). Both go out to Relay and back.
- **Truly different networks:** ship **the same build** to both players — Host vs Join is a
  runtime menu choice, not a separate build. One clicks **Host**, the other **Refresh ▸ pick a
  lobby**. Because all traffic is relayed, neither needs to open ports. (The only thing that forces
  a different build is platform: a WebGL build must set `Relay Connection Type` to `wss` instead of
  `dtls`; two desktop players are always identical.)
- **Multiplayer PlayMode Tools:** the old "Client & Server auto-spawns on Play" behaviour is gone
  by design. To temporarily restore localhost auto-connect for offline iteration, follow the note
  in `GameBootstrap.cs` (set `AutoConnectPort = 7979` and `return base.Initialize(...)`).

## 7. Costs / limits

Relay's free tier covers small-scale playtesting; sustained bandwidth and concurrent users are
metered on the dashboard. Lobby is free. For 1-v-1 sessions you'll stay comfortably within the
free tier during development. Monitor usage under the project's **Relay** dashboard.
