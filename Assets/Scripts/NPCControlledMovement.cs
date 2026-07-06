using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class NPCControlledMovement : MonoBehaviour
{
    Combat combat;
    public List<PlayerControlledMovement> players;
    UnitInfo _unitInfo;
    [SerializeField]
    float mininumApproachDistance;
    public Vector3 movementLocation, spawningLocation;
    NavMeshAgent _navMeshAgent;
    public float huntingDistance;
    [SerializeField]
    float leashRange;
    public byte id;
    [SerializeField]
    InputAction testMovement;
    public int hostConfirmedTarget;

    void Awake()
    {
        _unitInfo = GetComponent<UnitInfo>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hostConfirmedTarget = -1;
        testMovement.Enable();
        players = FindObjectsByType<PlayerControlledMovement>().ToList();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        combat = GetComponent<Combat>();
        movementLocation = transform.position;
        spawningLocation = transform.position;
        Debug.Log("Players count: " + players.Count);
    }

    // Update is called once per frame
    void Update()
    {
        if(_unitInfo.Target == null)
        {

            for(int i = 0; i < players.Count; i++)
            {
                if(Vector3.Distance(spawningLocation, players[i].transform.position) < leashRange/2f && Vector3.Distance(players[i].transform.position, transform.position) < huntingDistance)
                {
                    if(NetworkManager.Instance.is_host == 1)
                    {
                        _unitInfo.Target = players[i].gameObject;
                        NetworkManager.Instance.SendNPCTargetPacket(id, i);
                    }
                    else if(hostConfirmedTarget > -1)
                    {
                        _unitInfo.Target = players[hostConfirmedTarget].gameObject;
                        hostConfirmedTarget = -1;
                    }
                    else
                    {
                        NetworkManager.Instance.SendNPCTargetPacket(id, i, false);
                    }
                }
            }
        }
        if(_unitInfo.Target != null && _unitInfo.DistanceFromTarget > combat.baseAttackRange)
        {
            if (combat.isAttacking && _unitInfo.DistanceFromTarget > combat.attackRange)
            {
                movementLocation = transform.position;
                // Debug.Log("Movement Location: Me");
            }
            else
            {
                movementLocation = _unitInfo.Target.transform.position;
                // Debug.Log("Movement Location: Them");
            }
        }
        if(Vector3.Distance(spawningLocation, transform.position) > leashRange && _unitInfo.DistanceFromTarget > combat.attackRange || _unitInfo.Target != null && _unitInfo.Target.activeSelf == false)
        {
            Deselect();
            hostConfirmedTarget = -1;
        }
        HandleMovement();
    }

    private void Deselect()
    {
        _unitInfo.Target = null;
        movementLocation = spawningLocation;
        // Debug.Log("Movement Location: Home");
    }

    private void HandleMovement()
    {
        if (_unitInfo.Target != null)
        {
            _unitInfo.DistanceFromTarget = Vector3.Distance(transform.position, _unitInfo.Target.transform.position);
            if(combat.isAttacking && _unitInfo.DistanceFromTarget < combat.attackRange || _unitInfo.DistanceFromTarget < mininumApproachDistance)
            {
                // Debug.Log("Movement Location: Me");
                movementLocation = transform.position;
            }
        }
        _navMeshAgent.SetDestination(movementLocation);
        // if(NetworkManager.Instance.is_host == 1)
        // {
        //     NetworkManager.Instance.SendLocationPacket(id, movementLocation);
        // }
        // else
        // {
        //     NetworkManager.Instance.SendLocationPacket(id, movementLocation, false);
        // }
    }
    public void ClientUpdateMovementLocation(Vector3 location)
    {
        movementLocation = location;
    }
    public void ClientUpdateTarget(int targetId)
    {
        hostConfirmedTarget = targetId;
    }

}
