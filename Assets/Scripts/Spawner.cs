// using Mirror;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Spawner : NetworkBehaviour
// {

//     [SerializeField]
//     private EnemySpawner es;

//     private Camera cam;

//     [SerializeField]
//     private LayerMask walkableTerrain;
//     private void Start()
//     {
//         cam = Camera.main;
//         es = GetComponent<EnemySpawner>();
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         if (isServer)
//         {
//             if (Input.GetKeyDown(KeyCode.F))
//             {
//                 Vector3 worldMousePosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 200f));
//                 Vector3 direction = worldMousePosition - cam.transform.position;

//                 RaycastHit hit;

//                 if (Physics.Raycast(cam.transform.position, direction, out hit, 200f, walkableTerrain))
//                 {
//                     es.SpawnEnemy(hit.point);

//                 }
//             }
//         }
//     }
// }
