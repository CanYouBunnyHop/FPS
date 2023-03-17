using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CooldownData", menuName = "ability/Cooldown")]
public class cooldownDataSO : ScriptableObject
{
    
    public float cdTime;
    //public countMethod count_Method;

    [Header("Debug")]
    public float cdTimer;
    public bool canUseAbility;
    public bool isUsing = false;

    public void AwakeTimer()
    {
        
        cdTimer = 0;
        isUsing = false;
      
    }
    public void CoolingDown()
    {
      
        //Cool down
         canUseAbility = (cdTimer <= 0 && !isUsing); //if cooldown is ready and player is not using ability
        {
            //canUseAbility = true;
        }
        
        if(cdTimer > 0)
        cdTimer -= Time.fixedDeltaTime;
        
        
    }
    public void InitiateCoolDown()
    {
        cdTimer = cdTime;
    }
}
