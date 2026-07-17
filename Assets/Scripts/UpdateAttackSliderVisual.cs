
using FantasyIsland;
using UnityEngine;
using UnityEngine.UI;

public class UpdateAttackSliderVisual : MonoBehaviour
{
    
    [SerializeField]
    public Slider attackSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        attackSlider.value = GetComponent<NameplateElement>().AttackSlider;
    }
}