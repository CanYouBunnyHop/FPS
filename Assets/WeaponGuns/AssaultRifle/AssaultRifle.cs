using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifle : GunBehaviour
{
    public GameObject Grenade;
    public float altVerRecoil;
    public float altFirePower;
    public float altUpFirePower;
    public cooldownData ARgrenadeData;

    //Changing firemode for this gun will break script since things are hard coded, maybe need to rewrite more code? But will be difficult because each guns are unique

    public override void BehaviorFixedUpdate()
    {
        ARgrenadeData.CoolingDown();

        canShoot = !gunData.isReloading && timeSinceLastShot > timeBetweenShots && gunData.currentAmmo > 0;

        //calc timeSicelastShot
        timeSinceLastShot += Time.deltaTime;

        if(FireIAIQ.Count > 0) //if there are action items in queue
        {
            FireInputActionItem action = FireIAIQ.Peek();
            switch(action.fireIAI)
            {
                case FireInputActionItem.fireActionItem.FireAction:
                {

                    if(canShoot)
                    {
                        Shoot();
                        FireIAIQ.Dequeue(); Debug.Log("dequeue");
                        canShoot = false;
                        timeSinceLastShot = 0;
                    }
                    
                }
                break;
                case FireInputActionItem.fireActionItem.AltFireAction:
                {
                    if(ARgrenadeData.canUseAbility)
                    {
                        AltShoot();
                        FireIAIQ.Dequeue();
                        ARgrenadeData.canUseAbility = false;
                        ARgrenadeData.InitiateCoolDown();
                    }
                }
                break;
            }
        }
         if(timeSinceLastShot > timeBetweenShots + gunData.returnDelay) //recoil rest
        {
            dX = Mathf.SmoothStep(dX, 0,  timeSinceLastShot);
            dY = Mathf.SmoothStep(dY, 0,  timeSinceLastShot);

            shootTimes = 0;
        }
        //base.BehaviorFixedUpdate();
    }

    #region Input
    protected override void EnqueueShootInput(GunData.FireMode _fireMode, int? _fireInput)
    {
         switch(_fireMode)
        {
            case GunData.FireMode.FullAuto: //shoot
            {
                if( gunData.currentAmmo > 0 && timeSinceLastShot > timeBetweenShots)
                {
                    Shoot();
                    canShoot = false;
                    timeSinceLastShot = 0;
                }

            }
            break;

            case GunData.FireMode.SemiAuto: //altshoot
            {
                if(ARgrenadeData.cdTimer > ARgrenadeData.cdTime - 0.1f)
                {
                    FireInputActionItem item = new FireInputActionItem((FireInputActionItem.fireActionItem)_fireInput);
                    FireIAIQ.Enqueue(item); 
                }
            }
            break;
        }
        //base.EnqueueShootInput(_fireMode, _fireInput);
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
        GameObject nade = Instantiate<GameObject>(Grenade , cam.transform.position, cam.transform.rotation);
        Rigidbody nadeRB = nade.AddComponent<Rigidbody>();
        nadeRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Vector3 force = new Vector3(-altUpFirePower, 0, altFirePower);
        nadeRB.AddRelativeForce(force);

        //recoil
        recoilManager.targetRot += new Vector3(-altVerRecoil, 0);


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
