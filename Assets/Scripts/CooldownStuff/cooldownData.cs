using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CooldownData", menuName = "ability/Cooldown")]
public class cooldownData : ScriptableObject
{
    public float cdTime;

    [Header("Debug")]
    public float cdTimer;
    public bool canUseAbility;
    public bool isUsing = false;

    public void CoolingDown()
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
    public void InitiateCoolDown()
    {
        cdTimer = cdTime;
    }
}
