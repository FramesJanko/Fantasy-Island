using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextScript : MonoBehaviour
{
    // Start is called before the first frame update
    public int time;
    
    private int timeToSubtract;

    public bool readyToShowTime;
    public bool needToSetFirstTime;



    // Update is called once per frame
    void Update()
    {
        SetTime();

        // if (readyToShowTime)
        ShowTime();

    }

    private void ShowTime()
    {
        GetComponent<TextMeshProUGUI>().text = time.ToString();
    }

    private void SetTime()
    {
        if (needToSetFirstTime)
        {
            timeToSubtract = Mathf.RoundToInt(Time.time);
            needToSetFirstTime = false;
        }
        time = Mathf.RoundToInt(Time.time) - timeToSubtract;
        
    }
    
}
