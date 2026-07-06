using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    InputAction cameraPan;
    [SerializeField]
    InputAction cameraZoom;
    [SerializeField]
    private float cameraSpeed;
    [SerializeField]
    private float zoomSpeed = 0.01f;

    private Vector3 startPosition;
    private Vector3 newCameraPosition;
    private float targetZoomY;
    private const float zoomFloor = 2f;

    void Start()
    {
        cameraPan.Enable();
        cameraZoom.Enable();
        targetZoomY = Camera.main.transform.position.y;
    }

    void Update()
    {
        float scrollDelta = cameraZoom.ReadValue<float>();
        if (scrollDelta != 0f)
        {
            targetZoomY -= scrollDelta * zoomSpeed;
            targetZoomY = Mathf.Max(targetZoomY, zoomFloor);
        }

        Vector3 pos = Camera.main.transform.position;
        pos.y = Mathf.Lerp(pos.y, targetZoomY, Time.deltaTime * 10f);
        Camera.main.transform.position = pos;

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
