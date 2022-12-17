using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifle : GunBehaviour
{
    #region Input
    protected override void EnqueueShootInput(GunData.FireMode _fireMode, int? _fireInput)
    {
        base.EnqueueShootInput(_fireMode, _fireInput);
    }
    protected override void ReloadInput()
    {
        base.ReloadInput();
    }
    #endregion

    #region  Shooting Behaviors
    protected override void Shoot()
    {
        RaycastHit hit;
        gunData.currentAmmo --;
        base.Shoot();

        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, Mathf.Infinity, groundEnemyMask, QueryTriggerInteraction.Ignore))
        {
            BulletHoleFx(hit);
            if(hit.distance <= gunData.range)
            {
                //enemyHP - damage
            
            }
            else
            {
                float dropOff = hit.distance - gunData.range;
                dropOff = Mathf.Clamp(dropOff, 0, gunData.damage/3);
                //enemyHP - (damge - dropOff)
            }
        }
    }
    protected override void AltShoot()
    {
        
    }
    #endregion

    #region reload
    protected override IEnumerator Reload()
    {
       return base.Reload();
    }
    protected override void CancelReload(Coroutine IEReload)
    {
        base.CancelReload(IEReload);
    }
    #endregion
}
