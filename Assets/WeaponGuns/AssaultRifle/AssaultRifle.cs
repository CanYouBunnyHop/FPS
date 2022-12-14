using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifle : GunBehaviour
{
    
    public override void BehaviorInputUpdate()
    {
        base.BehaviorInputUpdate();
    }

    #region Input
    protected override void ShootInput(FireMode _fireMode, int? _fireInput)
    {
        base.ShootInput(_fireMode, _fireInput);
    }
    protected override void ReloadInput()
    {
        base.ReloadInput();
    }
    #endregion

    #region  Shooting Behaviors
     public override void Shoot()
    {

    }
    public override void AltShoot()
    {
        
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
}
