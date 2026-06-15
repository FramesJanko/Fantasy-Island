using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPlayer : MonoBehaviour
{
    [SerializeField]
    Player localPlayer;

    [SerializeField]
    private Material basicMaterial;

    [SerializeField]
    private Material transparentMaterial;

    

    Camera cam;
    void Start()
    {
        cam = Camera.main;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTransparentMaterial()
    {
        gameObject.GetComponent<MeshRenderer>().material = transparentMaterial;
    }

    public void SetOpaqueMaterial()
    {
        gameObject.GetComponent<MeshRenderer>().material = basicMaterial;
    }
    //private void ShowBehindWalls()
    //{
    //    Ray ray = new Ray(cam.transform.position, localPlayer.gameObject.transform.position);
    //    RaycastHit hit;
    //    MeshRenderer hitMeshRenderer;
    //    if (Physics.Raycast(ray, out hit, 200f))
    //    {
    //        hitMeshRenderer = hit.collider.gameObject.GetComponent<MeshRenderer>();
    //        hitMeshRenderer.material = transparentMaterial;

    //    }
    //    else
    //    {
    //        hitMeshRenderer = hit.collider.gameObject.GetComponent<MeshRenderer>();
    //        hitMeshRenderer.material = basicMaterial;
    //    }
    //}
}
