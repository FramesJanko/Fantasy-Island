using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    public static Spawner Instance;

    [SerializeField]

    private Camera cam;
    public GameObject player_prefab;
    public GameObject del_prefab;
    [SerializeField]
    private LayerMask walkable_terrain;
    private void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Awake()
    {
        Instance = this;
        NPCManager.npcs.Clear();
    }
    void Update()
    {

    }

    // Button hook: request a player spawn. The host spawns directly; a client
    // only asks the host (which assigns ownership to the requesting client).
    public void SpawnPlayerCharacter()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestSpawn(SpawnId.Player, transform.position);
        }
    }
    // Button hook: request an NPC (del) spawn, owned by no player.
    public void SpawnNPC()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestSpawn(SpawnId.Del, transform.position);
        }
    }

    // Host-authoritative spawn of a player owned by the given client (-1 = host).
    public GameObject SpawnPlayer(int ownerClientId, Vector3 position)
    {
        
        GameObject new_player = Instantiate(player_prefab, new Vector3(position.x + 1f, position.y, position.z), player_prefab.transform.rotation);
        PlayerControlledMovement movement = new_player.GetComponent<PlayerControlledMovement>();
        if (movement != null)
        {
            movement.ownerClientId = ownerClientId;
        }
        NPCManager.AddToTargetList(movement);
        return new_player;
    }
    // Host-authoritative spawn of an NPC (del) that no player owns.
    public GameObject SpawnDel(Vector3 position)
    {
        GameObject new_NPC = Instantiate(del_prefab, position, del_prefab.transform.rotation);
        new_NPC.GetComponent<NPCControlledMovement>().id = (byte)NPCManager.npcs.Count;
        NPCManager.npcs.Add(new_NPC);
        Debug.Log("NPC Count: " + NPCManager.npcs.Count);
        return new_NPC;
    }
}
