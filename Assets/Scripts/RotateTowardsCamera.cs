using UnityEngine;

public class RotateTowardsCamera : MonoBehaviour
{
    Camera _camera;
    public float maxRadians;
    public float maxMagnitudeDelta;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(new Vector3 (transform.position.x, _camera.transform.position.y, _camera.transform.position.z), Vector3.up);
        // Vector3 newRotation = Vector3.RotateTowards(transform.position, _camera.transform.position - transform.position, maxRadians, maxMagnitudeDelta);
        // transform.rotation = Quaternion.LookRotation(newRotation);
    }
}
