using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField]
    GameObject selectedObject;
    Health selectedHealth;
    [SerializeField]
    int healthText;
    public TextMeshProUGUI healthTMP;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectedHealth = selectedObject.GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        healthText = Mathf.RoundToInt(selectedHealth.currentHealth);
        healthTMP.text = healthText.ToString();
    }
}
