using System.Collections.Generic;
using UnityEngine;

// Cross-machine registry of spawned players. Ownership (ownerClientId) is assigned
// identically on the host and every client during the spawn handshake, so it is a
// stable key for finding "the player owned by X" on any machine when a movement
// packet arrives. -1 = the host's player; >= 0 = that client's player.
public static class PlayerManager
{
    public static Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    public static void Register(int ownerClientId, GameObject player)
    {
        players[ownerClientId] = player;
    }

    public static bool TryGetPlayer(int ownerClientId, out GameObject player)
    {
        return players.TryGetValue(ownerClientId, out player);
    }

    public static void Clear()
    {
        players.Clear();
    }
}
