using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    [SerializeField]
    private float healthChangeTimer;

    [SerializeField]
    private Image healthbarImage;

    public float currentHealthPercent;

    private void OnEnable()
    {
        GetComponentInParent<Health>().EventHealthChanged += CmdChangeHealthbar;
        currentHealthPercent = healthbarImage.fillAmount;
    }

    
    private void CmdChangeHealthbar(float percent)
    {
        StartCoroutine(HealOrDamage(percent));
    }
    
    public void HealOrDamageWithoutEnumerator()
    {
        float preChangePercent = healthbarImage.fillAmount;
        float timeElapsed = healthChangeTimer;
        
        if (timeElapsed < 0)
        {
            timeElapsed = 0;
        }

        if (preChangePercent != currentHealthPercent && timeElapsed >= 0)
        {
            timeElapsed -= Time.deltaTime;
            healthbarImage.fillAmount = currentHealthPercent + (preChangePercent - currentHealthPercent) * (timeElapsed / healthChangeTimer);
            //Mathf.Lerp(preChangePercent, percent, timeElapsed / healthChangeTimer);
        }
    }

    private IEnumerator HealOrDamage(float percent)
    {
        float preChangePercent = healthbarImage.fillAmount;
        float timeElapsed = 0f;

        while (timeElapsed < healthChangeTimer)
        {
            timeElapsed += Time.deltaTime;
            healthbarImage.fillAmount = Mathf.Lerp(preChangePercent, percent, timeElapsed / healthChangeTimer);
            yield return null;
        }

        healthbarImage.fillAmount = percent;

    }
    
    public void UpdateCurrentHealthPercent(float percent)
    {
        currentHealthPercent = percent;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        healthbarImage.fillAmount = GetComponentInParent<Health>().currentHealth / GetComponentInParent<Health>().maxHealth;
        //HealOrDamageWithoutEnumerator();
    }
    void LateUpdate()
    {
        //transform.LookAt(Camera.main.transform);
        //transform.Rotate(0, 180, 0);
    }
}
