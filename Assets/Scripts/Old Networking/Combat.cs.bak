using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class Combat : MonoBehaviour
{
    private UnitInfo unitInfo;
    [SerializeField]
    public float baseAttackTime;
    public float AttackAnimatorNormalizedTime;
    public float attackBackswing;
    [SerializeField]
    public float attackRange;
    [SerializeField]
    private float damage;
    // public float distanceFromTarget;
    public bool isAttacking;
    private bool hasAnimator;
    public bool attackIsCanceled;
    private bool isPlayer;
    private int coroutineCount = 0;
    public Animator animator;
    IEnumerator AttackCoroutine;
    [SerializeField]
    public float baseAttackRange;
    public bool attackIsValid;
    public float attackProgressInSeconds;


    // Start is called before the first frame update
    void Start()
    {
        unitInfo = GetComponent<UnitInfo>();
        isPlayer = unitInfo;
        // if (GetComponent<NpcController>())
        // {
        //     npcController = GetComponent<NpcController>();
        //     isPlayer = false;
        // }

        // if (GetComponent<Animator>())
        // {
        //     hasAnimator = true;
        //     animator = GetComponent<Animator>();

        // }
    }

    // Update is called once per frame
    void Update()
    {

        if (isAttacking)
        {
            attackProgressInSeconds += Time.deltaTime;
        }
        else
        {
            attackProgressInSeconds = 0f;
        }
        if (isPlayer)
        {
            attackIsValid = CheckValidTarget(unitInfo.Target);        
            if (attackIsValid)
            {
                StartAttack();
            }
        }

        // if (hasAnimator)
        // {
        //     animator.SetFloat("speedMultiplier", attackspeed);
        //     AttackAnimatorNormalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        // }

        // if (CheckValidTarget(target))
        // {
        //     BeginAnimatedAttack();
        //     //StartAttack();
        // }
        // if (AttackAnimatorNormalizedTime == baseAttackTime)
        // {
        //     CmdModifyHealth(target, damage, 0f);
        // }
        // if (AttackAnimatorNormalizedTime == baseAttackTimeAndBackSwing)
        // {
        //     StopAttack();
        // }

        // //CalculateAttackSpeed();
        
        // if (shouldAnimateAttack)
        // {
        //     if (hasAnimator && !animator.GetBool("IsAttacking"))
        //     {
        //         animator.SetBool("IsAttacking", true);

        //     }
        // }
        // else
        // {
        //     if (hasAnimator)
        //     {
        //         animator.SetBool("IsAttacking", false);
        //     }
        //     CancelAttack();

        // }
    }


    public void ResolveAttackHit()
    {
        Debug.Log("Attack Hit!");
    }
    private void BeginAnimatedAttack()
    {
        // shouldAnimateAttack = true;
        // if (isPlayer)
        // {
        //     attackFinished = false;
        //     shouldAnimateAttack = true;
        //     isAttacking = true;
        //     attackIsCanceled = false;
        // }
        // else if (!isPlayer)
        //     NPCStartAttack();
    }



    public void CmdCalculateAttackSpeed()
    {
        // baseAttackTimeAndBackSwing = (1 / (1 / initialBaseAttackTimeAndBackSwing * attackspeed));
        // baseAttackTime = .8f * baseAttackTimeAndBackSwing;
        // attackBackswing = .2f * baseAttackTimeAndBackSwing;

        // //if (hasAnimator)
        // //    animator.SetFloat("speedMultiplier", attackspeed);
    }
    private void CalculateAttackSpeed()
    {
        // baseAttackTimeAndBackSwing = (1 / (1 / initialBaseAttackTimeAndBackSwing * attackspeed));
        // baseAttackTime = .5f * baseAttackTimeAndBackSwing;
        // attackBackswing = .5f * baseAttackTimeAndBackSwing;


    }

    private void StartAttack()
    {

        if (AttackCoroutine != null)
            StopCoroutine(AttackCoroutine);


        AttackCoroutine = Attack();
        StartCoroutine(AttackCoroutine);


    }

    public void CancelAttack()
    {
        attackIsCanceled = true;
        isAttacking = false;
    }
    private bool CheckValidTarget(GameObject currentTarget)
    {
        bool TargetValid;
        if (currentTarget != null && unitInfo.DistanceFromTarget < baseAttackRange && !isAttacking)
        {
            // if(name != "Art")
            // {
            //     Debug.Log(name + " Is attacking: " + isAttacking);
            //     Debug.Log(name + " Target exists, close and not attacking");
            //     Debug.Log(name + " Distance: " + unitInfo.DistanceFromTarget);
            //     Debug.Log(name + " Base Range: " + baseAttackRange);
            // }
            TargetValid = true;
        }
        
        else if (currentTarget != null && unitInfo.DistanceFromTarget < attackRange && isAttacking)
        {
            // Debug.Log(name + " Is attacking: " + isAttacking);
            // Debug.Log(name + " Target exists, close and is attacking");
            // Debug.Log(name + " Distance: " + unitInfo.DistanceFromTarget);
            // Debug.Log(name + " Far Range: " + attackRange);
            TargetValid = false;
        }

        else if (currentTarget != null && unitInfo.DistanceFromTarget > attackRange)
        {
            // if (hasAnimator)
            // {
            //     animator.SetBool("IsAttacking", false);
            // }
            // Debug.Log(name + " Target exists, too far");
            // Debug.Log(name + " Distance: " + unitInfo.DistanceFromTarget);
            // Debug.Log(name + " Far Range: " + attackRange);
            isAttacking = false;
            attackIsCanceled = true;
            TargetValid = false;
        }
        // else if (currentTarget != null && distanceFromTarget > attackRange)
        // {
        //     if (isPlayer && isAttacking)
        //     {
        //         StopAttack();
        //     }
        //     else if (!isPlayer && isAttacking)
        //     {
        //         NPCStopAttack();
        //     }

        // }
        else if (unitInfo.Target == null)
        {
            // if (hasAnimator)
            // {
            //     animator.SetBool("IsAttacking", false);
            // }
            // Debug.Log(name + " Target doesn't exist");
            isAttacking = false;
            attackIsCanceled = true;
            TargetValid = false;

            // if (isPlayer)
            // {
            //     StopAttack();
            // }
            // else
            // {
            //     NPCStopAttack();
            // }
        }
        else
        {
            // Debug.Log(name + " unknown");
            // Debug.Log(name + " Distance: " + unitInfo.DistanceFromTarget);
            // Debug.Log(name + " Base Range: " + baseAttackRange);
            // Debug.Log(name + " Is attacking: " + isAttacking);
            // Debug.Log(name + " Far Range: " + attackRange);
            TargetValid = false;
        }
        if (unitInfo.Walking)
        {

            // if (hasAnimator)
            // {
            //     animator.SetBool("IsAttacking", false);
            // }
            // Debug.Log(name + " Walking");
            isAttacking = false;
            attackIsCanceled = true;
            TargetValid = false;
        }
        return TargetValid;
    }
    public void CmdModifyHealth(GameObject currentTarget, float healthChange)
    {
        currentTarget.GetComponent<Health>().currentHealth += healthChange;
    }



    private void NPCStopAttack()
    {
        // //Debug.Log(name + " NPCStopAttack");
        // isAttacking = false;
        // timeSinceLastSuccessfulAttack = attackBackswing;
        // attackFinished = true;
        // incrementAttackTimer = false;
        // attackTimer = 0f;
    }

    //public void AnimateAttack()
    //{
    //    attackFinished = false;
    //    shouldAnimateAttack = true;
    //    isAttacking = true;
    //    attackIsCanceled = false;
    //}
    // public void AnimateAttackServer()
    // {
    //     attackFinished = false;
    //     shouldAnimateAttack = true;
    //     isAttacking = true;
    //     attackIsCanceled = false;
    // }

    public void StopAnimateAttack()
    {
        // //Debug.Log("StopAnimateAttack called");
        // shouldAnimateAttack = false;
    }
    public void StopAnimateAttackServer()
    {
        // //Debug.Log("StopAnimateAttack called");
        // shouldAnimateAttack = false;
    }
    public void StopAttack()
    {
        //if (!isServer)
        //    return;





        // Debug.Log("StopAttack called");
        // attackIsCanceled = true;
        // isAttacking = false;
        // shouldAnimateAttack = false;
        // attackFinished = true;
    }
    public void StopAttackServer()
    {


        //float newHealthPercent = currentTarget.GetComponent<Health>().currentHealth / currentTarget.GetComponent<Health>().maxHealth;

        //currentTarget.GetComponentInChildren<Healthbar>().currentHealthPercent = newHealthPercent;

        //Remove(currentTarget);
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        attackIsCanceled = false;
        coroutineCount++;
        // if (hasAnimator)
        // {

        //     animator.SetBool("IsAttacking", true);

        //     Debug.Log("AnimateAttack called");
        //     //AnimateAttack();
        //     AnimateAttackServer();

        // }

        GameObject currentTarget = unitInfo.Target;
        yield return new WaitForSeconds(baseAttackTime);


        if (unitInfo != null)
        {
            if(unitInfo.Target != null)
            {

                // Debug.Log(player.distanceFromTarget < attackRange);
                // Debug.Log(!player.walking);
                // Debug.Log(currentTarget == player.target);
                // Debug.Log(player.target.activeSelf);
                // Debug.Log(gameObject.activeSelf);
                // Debug.Log(!attackIsCanceled);
                if (unitInfo.DistanceFromTarget < attackRange && !unitInfo.Walking && currentTarget == unitInfo.Target && unitInfo.Target.activeSelf && gameObject.activeSelf && !attackIsCanceled)
                {
                    // Debug.Log(name + " attacked successfully");
                    unitInfo.Target.GetComponent<Health>().ModifyHealth(-damage);
                    // if (isPlayer)
                    // {
                    //     CmdModifyHealth(currentTarget, damage, 0f);
                    //     Debug.Log(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                    // }
                    // if (!isPlayer && isServer)
                    // {
                    //     currentTarget.GetComponent<Health>().currentHealth += damage;
                    // }

                }
            }
        }


        isAttacking = false;
    }

    private void NPCStartAttack()
    {
        // //Debug.Log(name + " NPCStartAttack");
        // isAttacking = true;
        // attackFinished = false;
        // incrementAttackTimer = true;

    }

    //public void Remove(GameObject currentTarget)
    //{

    //    if (currentTarget.GetComponent<Health>().currentHealth <= 0f)
    //    {
    //        Player[] playerList = GetComponents<Player>();
    //        foreach (Player p in playerList)
    //        {
    //            if (p.target == gameObject)
    //            {
    //                p.target = null;
    //            }
    //        }
    //        Destroy(currentTarget);
    //    }
    //}





}