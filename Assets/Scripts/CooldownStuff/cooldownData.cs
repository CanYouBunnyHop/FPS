using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CooldownData", menuName = "ability/Cooldown")]
public class cooldownData : ScriptableObject
{
    
    public float cdTime;
    public countMethod count_Method;

    [Header("Debug")]
    public float cdTimer;
    public bool canUseAbility;
    public bool isUsing = false;

    public enum countMethod
    {
        CountDown,
        CountUp
    }
    public void AwakeTimer()
    {
        if(count_Method == countMethod.CountDown)
        {
            cdTimer = 0;
        }
        if(count_Method == countMethod.CountUp)
        {
            cdTimer = cdTime;
        }
    }
    public void CoolingDown()
    {
        switch(count_Method)
        {
            case countMethod.CountDown:
            {
                //Cool down
                if (cdTimer <= 0 && !isUsing) //if cooldown is ready and player is not using ability
                {
                    canUseAbility = true;
                }
                else
                {
                    canUseAbility = false;
                    cdTimer -= Time.fixedDeltaTime;
                }
            }
            break;
            case countMethod.CountUp:
            {
                if(cdTimer>= cdTime && !isUsing)
                {
                    canUseAbility = true;
                }
                else
                {
                    canUseAbility = false;
                    cdTimer += Time.fixedDeltaTime;
                }
            }
            break;
        }
        
    }
    public void InitiateCoolDown()
    {
        if(count_Method == countMethod.CountDown)
        cdTimer = cdTime;

        else
        cdTimer = 0;
    }
}
