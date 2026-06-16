using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    InputAction cameraPan;
    [SerializeField]
    private float cameraSpeed;

    private Vector3 startPosition;

    private Vector3 newCameraPosition;
    // Start is called before the first frame update
    void Start()
    {
        cameraPan.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraPan.WasPressedThisFrame())
        {
            startPosition = Mouse.current.position.ReadValue();
        }
        if (cameraPan.IsPressed())
        {
            
            Vector3 changingMousePosition = Mouse.current.position.ReadValue();
            float timeElapsed = 0f;
            timeElapsed += Time.deltaTime;

            newCameraPosition = Camera.main.transform.position;

            newCameraPosition.x += -1f * ((startPosition.x - changingMousePosition.x) / (1/cameraSpeed));
            newCameraPosition.z += -1f * ((startPosition.y - changingMousePosition.y) / (1/cameraSpeed));
            
            
            Camera.main.transform.position = newCameraPosition;
            startPosition = Mouse.current.position.ReadValue();
            // while (timeElapsed < cameraSpeed)
            // {
            //    Camera.main.transform.position = Mathf.Lerp(Camera.main.transform.position, newCameraPosition, timeElapsed / cameraSpeed);
            // }

        }
    }
}
