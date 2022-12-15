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
