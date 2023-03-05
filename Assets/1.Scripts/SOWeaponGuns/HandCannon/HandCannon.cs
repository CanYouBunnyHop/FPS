using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCannon : GunBehaviour
{
#region Input
  protected override void EnqueueShootInput(GunDataSO.FireMode _fireMode, int? _fireInput)
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

      if(Physics.Raycast(cam.transform.position, aimDir, out hit, Mathf.Infinity, (enemyMask | groundMask), QueryTriggerInteraction.Ignore))
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
    protected override void SpecialShoot()
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
    
    //animations
#region  animations
    public override void Anim_Shoot()
    {

    }
    public override void Anim_SpecialShoot()
    {

    }
    public override void Anim_Reload()
    {

    }
#endregion
}
