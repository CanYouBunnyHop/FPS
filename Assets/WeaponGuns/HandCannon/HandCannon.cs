using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCannon : GunBehaviour
{
  public override void BehaviorInputUpdate()
    {
        base.BehaviorInputUpdate();
    }
    protected override void ShootInput(FireMode _fireMode, int? _fireInput)
    {
        base.ShootInput(_fireMode, _fireInput);
    }
   
    protected override void ReloadInput()
    {
        base.ReloadInput();
    }
    //  public override void QueueShoot(int? _fireInput)
    // {
    //     base.QueueShoot(_fireInput);
    // }
    // public override void QueueAltShoot()
    // {
    //    base.QueueAltShoot();
    // }
    #region  Shooting Behaviors
    public override void Shoot()
    {
        RaycastHit hit;
        gunData.currentAmmo --;
        //startQshoot = false;

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
    #endregion

    #region reload
    public override IEnumerator Reload()
    {
        return base.Reload();
    }
    public override void CancelReload(Coroutine IEReload)
    {
        base.CancelReload(IEReload);
    }
    #endregion
    
    //animations
    #region  animations
    public override void Anim_Shoot()
    {

    }
    public override void Anim_AltShoot()
    {

    }
    public override void Anim_Reload()
    {

    }
    #endregion
}
