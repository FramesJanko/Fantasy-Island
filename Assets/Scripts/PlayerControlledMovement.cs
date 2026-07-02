using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerControlledMovement : MonoBehaviour
{
    [Header("Networking")]
    [Tooltip("-1 = owned by the host; >= 0 = owned by that client id.")]
    public int ownerClientId = -1;
    Combat combat;
    UnitInfo _unitInfo;
    Camera cam;
    public bool walking;
    private Vector3 movementLocation;
    public GameObject target;
    RaycastHit hit;
    public bool moveCommandIssued;
    public float distanceFromTarget;
    public Collider destinationCollider;
    NavMeshAgent _navMeshAgent;
    [SerializeField]
    InputAction rightClickMove;
    [SerializeField]
    InputAction stopMoving;
    [SerializeField]
    private float mininumApproachDistance;
    [SerializeField]
    LayerMask walkableTerrain;
    private Vector3 direction;
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _unitInfo = GetComponent<UnitInfo>();
        combat = GetComponent<Combat>();
    }
    void Start()
    {
        stopMoving.Enable();
        rightClickMove.Enable();
        movementLocation = transform.position;
        walking = false;
        _unitInfo.Walking = false;
        cam = Camera.main;
    }
    void Update()
    {
        if (stopMoving.WasPressedThisFrame())
        {
            SetStopCommand();
            Deselect();
        }
        if (rightClickMove.WasPressedThisFrame())
        {
            RayCastToFindTarget();
        }
        HandleMovement();
    }
    private void HandleMovement()
    {
        if (target != null)
        {
            distanceFromTarget = Vector3.Distance(transform.position, target.transform.position);
            if(target.activeSelf == false)
            {
                Deselect();
                movementLocation = transform.position;
            }
            else if(combat.isAttacking && distanceFromTarget < combat.attackRange || distanceFromTarget < mininumApproachDistance)
            {
                movementLocation = transform.position;
            }
            else if(distanceFromTarget > combat.baseAttackRange)
            {
                movementLocation = target.transform.position;
            }
        }
        _unitInfo.DistanceFromTarget = distanceFromTarget;
        _navMeshAgent.SetDestination(movementLocation);
    }
    private void SetStopCommand()
    {
        movementLocation = transform.position;
    }
    private void RayCastToFindTarget()
    {
        Vector3 worldMousePosition = cam.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 200f));
        direction = worldMousePosition - cam.transform.position;
        if (Physics.Raycast(cam.transform.position, direction, out hit, 200f, walkableTerrain))
        {
            Debug.DrawLine(cam.transform.position, hit.point, Color.green, 0.5f);
            CheckForTarget();
        }
        else
        {
            Debug.DrawLine(cam.transform.position, worldMousePosition, Color.red, 0.5f);
        }
    }
    private void CheckForTarget()
    {
        if (hit.collider.tag == "Enemy")
        {
            target = hit.collider.gameObject;
            _unitInfo.Target = hit.collider.gameObject;
            movementLocation = hit.collider.gameObject.transform.position;
        }
        else
        {
            Deselect();
            movementLocation = hit.point;
        }
        destinationCollider = hit.collider;
    }
    private void Deselect()
    {
        target = null;
        _unitInfo.Target = null;
    }
}