using FantasyIsland;
using UnityEngine;
using UnityEngine.UI;

public class UpdateHealthbarVisual : MonoBehaviour
{
    
    [SerializeField]
    public Slider healthSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = GetComponent<NameplateElement>().Health;
    }
}
