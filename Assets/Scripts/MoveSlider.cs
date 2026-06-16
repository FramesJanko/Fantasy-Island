using UnityEngine;
using UnityEngine.UI;

public class MoveSlider : MonoBehaviour
{
    public Slider attackTimeSlider;
    Combat combat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        combat = GetComponentInParent<Combat>();    
    }

    // Update is called once per frame
    void Update()
    {
        attackTimeSlider.value = combat.attackProgressInSeconds/combat.baseAttackTime;
    }
}
