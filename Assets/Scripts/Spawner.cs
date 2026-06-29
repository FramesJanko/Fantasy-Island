using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{

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
        NPCManager.npcs.Clear();
    }
    void Update()
    {

    }

    public void SpawnPlayerCharacter()
    {

        GameObject new_player = Instantiate(player_prefab, transform.position, player_prefab.transform.rotation);
        NPCManager.AddToTargetList(new_player.GetComponent<PlayerControlledMovement>());
    }
    public void SpawnNPC()
    {
        GameObject new_NPC = Instantiate(del_prefab, transform.position, del_prefab.transform.rotation);
        NPCManager.npcs.Add(new_NPC);
        Debug.Log("NPC Count: " + NPCManager.npcs.Count);
    }
}
