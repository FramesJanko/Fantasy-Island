// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Mirror;

// public class EnemySpawner : NetworkBehaviour
// {
//     //private NetworkConnection noConnection = null;

//     [SerializeField]
//     private GameObject enemyPrefab;
    
//     public void SpawnEnemy()
//     {
//         if (!isServer)
//             return;
//         GameObject enemy = Instantiate(enemyPrefab, transform.position, enemyPrefab.transform.rotation);
//         NetworkServer.Spawn(enemy);
//         enemy.GetComponent<NpcController>().origin = transform.position;
//         Debug.Log($"Spawning enemy..., rotation is {enemyPrefab.transform.rotation}");
//     }

//     public void SpawnEnemy(Vector3 spawnPoint)
//     {
//         if (!isServer)
//             return;
//         GameObject enemy = Instantiate(enemyPrefab, spawnPoint, enemyPrefab.transform.rotation);
//         NetworkServer.Spawn(enemy);
//         enemy.GetComponent<NpcController>().origin = spawnPoint;
//         Debug.Log($"Spawning enemy..., rotation is {enemyPrefab.transform.rotation}");
//     }

// }
