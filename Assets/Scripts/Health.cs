using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Health : MonoBehaviour
{
    [SerializeField]
    public float maxHealth = 100;

    public float currentHealth;

    public delegate void OnHealthPercentChanged(float currentHealthPercent);
    public Player[] playersList;
    public int playersInExpRange;

    public event OnHealthPercentChanged EventHealthChanged;

    [SerializeField]
    public float experianceRange;

    public int experiencePool;
    bool isPlayer;
    private void OnEnable()
    {
        currentHealth = maxHealth;
        if (GetComponent<Player>())
        {
            isPlayer = true;
        }
    }
    
    public void ModifyHealth(float healthChange)
    {
        currentHealth += healthChange;
    }

    void Update()
    {
        CheckForDeath();

    }

    private void CheckForDeath()
    {
        if(currentHealth <= 0)
        {
            Death();
        }
    }
    
    private void Death()
    {
        // Player[] playerList = FindObjectsByType<Player>();
        // foreach (Player p in playerList)
        // {
        //     if (p.target == gameObject)
        //     {
        //         p.target = null;
        //     }
        // }
        // GetListofPlayersInRange();
        //AwardExperience();
        gameObject.SetActive(false);
    }
    public void GetListofPlayersInRange()
    {
        playersList = FindObjectsByType<Player>();
        
        if (playersList.Length > 0)
        {
            
            foreach (Player p in playersList)
            {
                
                if (Vector3.Distance(transform.position, p.transform.position) <= experianceRange)
                {
                    playersInExpRange++;
                    
                }
                

            }
            foreach (Player p in playersList)
            {

                if (Vector3.Distance(transform.position, p.transform.position) <= experianceRange)
                {
                    
                    AwardExperience(p);
                }


            }
            Debug.Log("Players in experience range: " + playersInExpRange);
            playersInExpRange = 0;
        }
        
    }
    
    public void AwardExperience(Player p)
    {
        if (playersInExpRange == 1)
            p.totalExperience += experiencePool;
        
        if (playersInExpRange > 1)
            p.totalExperience += ((experiencePool / playersInExpRange) + 5);
        
        
    }
}
