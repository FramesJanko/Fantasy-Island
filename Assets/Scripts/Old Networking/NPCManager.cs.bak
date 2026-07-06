using System.Collections.Generic;
using UnityEngine;

public static class NPCManager
{
    public static List<GameObject> npcs = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static void AddToTargetList(PlayerControlledMovement pcm)
    {
        if(npcs.Count > 1)
        {
            Debug.Log("No npcs");
            return;
        }
        
        foreach(GameObject npc in npcs)
        {
            Debug.Log($"Working with {npc.name} located at {npc.gameObject.transform.position}");
            List<PlayerControlledMovement> players = npc.GetComponent<NPCControlledMovement>().players;
            if (!players.Contains(pcm))
            {
                players.Add(pcm);
                Debug.Log($"{npc.name} adding {pcm.name} to target list");
            }
        }
    }
}
