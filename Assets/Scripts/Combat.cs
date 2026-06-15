using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Combat : MonoBehaviour
{
    private Player player;
    // private NpcController npcController;

    public GameObject target;

    [SerializeField]
    private float baseAttackTime;

    // public float baseAttackTime;
    public float AttackAnimatorNormalizedTime;

    public float attackBackswing;

    // public float damage;

    [SerializeField]
    public float attackRange;

    [SerializeField]
    private float damage;
    public float distanceFromTarget;
    public bool walking;
    public bool isAttacking;
    private bool hasAnimator;
    public bool attackIsCanceled;
    private bool isPlayer;
    private int coroutineCount = 0;
    public Animator animator;
    IEnumerator AttackCoroutine;
    [SerializeField]
    public float baseAttackRange;



    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<Player>())
        {
            player = GetComponent<Player>();
            isPlayer = true;
        }
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

        // //if (!isLocalPlayer)
        // //    return;

        // if (isPlayer)
        // {
        //     distanceFromTarget = player.distanceFromTarget;
        //     walking = player.walking;
        //     target = player.target;
        // }

        // // if (!isPlayer)
        // // {
        // //     distanceFromTarget = npcController.distanceFromTarget;
        // //     walking = npcController.walking;

        // // }
        // if (CheckValidTarget(target))
        // {
        //     StartAttack();
        // }
        // if (Input.GetKeyDown(KeyCode.S))
        // {

        //     //attackTimer += Time.deltaTime;

        //     if (incrementAttackTimer)
        //     {
        //         attackTimer += Time.deltaTime;
        //     }
        // }
        // timeSinceLastSuccessfulAttack += Time.deltaTime;



        // if (isLocalPlayer && Input.GetKeyDown(KeyCode.S))
        // {
        //     //StopAttack();
        //     StopAttackServer();
        //     //StopAnimateAttack();
        //     StopAnimateAttackServer();
        // }
        // if (isLocalPlayer && Input.GetKey(KeyCode.Q))
        // {
        //     ModifyAttackSpeed(momentumAttackSpeedValue);
        //     CmdCalculateAttackSpeed();
        //     CalculateAttackSpeed();


        // }
        // if (hasAnimator)
        // {
        //     animator.SetFloat("speedMultiplier", attackspeed);
        // }

        // if (hasAnimator)
        // {
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

        // if (AttackCoroutine != null)
        //     StopCoroutine(AttackCoroutine);


        // AttackCoroutine = Attack();
        // StartCoroutine(AttackCoroutine);


    }

    public void CancelAttack()
    {
        attackIsCanceled = true;
        isAttacking = false;
    }
    private bool CheckValidTarget(GameObject currentTarget)
    {
        // bool TargetValid;
        // if (currentTarget != null && distanceFromTarget < baseAttackRange && !isAttacking)
        // {




        //     TargetValid = true;



        // }
        // else if (currentTarget != null && distanceFromTarget < attackRange && isAttacking && timeSinceLastSuccessfulAttack > attackBackswing && attackFinished)
        // {
        //     if (isPlayer)
        //     {
        //         StopAttack();
        //     }
        //     else
        //     {
        //         NPCStopAttack();
        //     }
        //     TargetValid = false;

        // }
        // else if (currentTarget != null && distanceFromTarget < attackRange && isAttacking)
        // {
        //     TargetValid = false;
        // }

        // else if (currentTarget != null && distanceFromTarget > attackRange)
        // {
        //     if (hasAnimator)
        //     {
        //         animator.SetBool("IsAttacking", false);
        //     }

        //     isAttacking = false;
        //     attackIsCanceled = true;
        // }
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

        //     TargetValid = false;
        // }
        // else if (target == null)
        // {

        //     if (hasAnimator)
        //     {
        //         animator.SetBool("IsAttacking", false);
        //     }
        //     attackIsCanceled = true;


        //     if (isPlayer)
        //     {

        //         StopAttack();
        //     }
        //     else
        //     {
        //         NPCStopAttack();
        //     }

        //     TargetValid = false;
        // }
        // else
        // {
        //     TargetValid = false;
        // }
        // if (walking)
        // {

        //     if (hasAnimator)
        //     {
        //         animator.SetBool("IsAttacking", false);
        //     }
        //     attackIsCanceled = true;

        //     TargetValid = false;

        // }
        // return TargetValid;
        return false;
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
        // isAttacking = true;
        // attackIsCanceled = false;
        // coroutineCount++;
        // if (hasAnimator)
        // {

        //     animator.SetBool("IsAttacking", true);

        //     Debug.Log("AnimateAttack called");
        //     //AnimateAttack();
        //     AnimateAttackServer();

        // }
        // Debug.Log("Attack Coroutine Started: " + coroutineCount);

        // GameObject currentTarget = target;
        yield return new WaitForSeconds(baseAttackTime);



        // if (distanceFromTarget < attackRange && !walking && currentTarget == target && target.activeSelf && gameObject.activeSelf && !attackIsCanceled)
        // {
        //     Debug.Log(name + " attacked successfully");
        //     if (isPlayer)
        //     {
        //         CmdModifyHealth(currentTarget, damage, 0f);
        //         Debug.Log(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        //     }
        //     if (!isPlayer && isServer)
        //     {
        //         currentTarget.GetComponent<Health>().currentHealth += damage;
        //     }

        // }


        // isAttacking = false;
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