using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NPCControlledMovement : MonoBehaviour
{
    Combat combat;
    PlayerControlledMovement[] players;
    UnitInfo _unitInfo;
    [SerializeField]
    float mininumApproachDistance;
    public Vector3 movementLocation, spawningLocation;
    NavMeshAgent _navMeshAgent;
    public float huntingDistance;
    [SerializeField]
    float leashRange;

    void Awake()
    {
        _unitInfo = GetComponent<UnitInfo>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        players = FindObjectsByType<PlayerControlledMovement>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        combat = GetComponent<Combat>();
        movementLocation = transform.position;
        spawningLocation = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(_unitInfo.Target == null)
        {

            foreach(PlayerControlledMovement player in players)
            {
                if(Vector3.Distance(spawningLocation, player.transform.position) < leashRange/2f && Vector3.Distance(player.transform.position, transform.position) < huntingDistance)
                {
                    _unitInfo.Target = player.gameObject;
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
    }
}
