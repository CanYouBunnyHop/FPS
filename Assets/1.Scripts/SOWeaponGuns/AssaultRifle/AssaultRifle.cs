using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPS.Player.Movement;
public class AssaultRifle : GunBehaviour
{
    public GameObject Grenade;
    public float altVerRecoil;
    public float altFirePower;
    public float altUpFirePower;

    [Tooltip("the forward distance the nade will shoot out of")] public float offsetFirePos;
    public cooldownDataSO ARgrenadeData;
    [Header("probably should be static")]
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private LayerMask nadeHitSurface;
    //[SerializeField] private VisualEffect mflash;

    //Changing firemode for this gun will break script since things are hard coded, maybe need to rewrite more code?
    //But will be difficult because each guns are unique

    public override void Behavior_FixedUpdate()
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
                case FireInputActionItem.fireActionItem.SpecialFireAction:
                {
                    if(ARgrenadeData.canUseAbility)
                    {
                        SpecialShoot();
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
    protected override void EnqueueShoot_Input(GunDataSO.FireMode _fireMode, int? _fireInput)
    {
        switch(_fireMode)
        {
            case GunDataSO.FireMode.FullAuto: //shoot
            {
                if( gunData.currentAmmo > 0 && timeSinceLastShot > timeBetweenShots)
                {
                    Shoot();
                    canShoot = false;
                    timeSinceLastShot = 0;
                }

            }
            break;

            case GunDataSO.FireMode.SemiAuto: //special Shoot
            {
                if(ARgrenadeData.cdTimer < 0.3f)
                {
                    FireInputActionItem item = new FireInputActionItem((FireInputActionItem.fireActionItem)_fireInput);
                    FireIAIQ.Enqueue(item); 
                }
            }
            break;
        }
        //base.EnqueueShootInput(_fireMode, _fireInput);
    }
    protected override void Reload_Input()
    {
        base.Reload_Input();
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
        //calc fire pos
        //if there are objects that will interact with the nade obstructing fire pos, change fire pos
        float realOffset = offsetFirePos;

        if(Physics.SphereCast(cam.transform.position, 0.3f ,cam.transform.forward, out RaycastHit hit, offsetFirePos))
        {
            if(hit.collider.gameObject.layer == nadeHitSurface)
            {
                realOffset = hit.distance - 0.1f; //if hit something set new pos
            }
        }
        //spawn nade
        GameObject nade = Instantiate<GameObject>(Grenade , cam.transform.position + realOffset * cam.transform.forward, cam.transform.rotation);

        //debug error
        if(!nade.TryGetComponent<Rigidbody>(out Rigidbody nadeRB))
        {
            Debug.Log("Nade don't have rigidbody");
        }

        //force
        float forceMultiplier = Mathf.Clamp(Vector3.Dot(pm.playerZXVel, cam.transform.forward), 0, 1); //make sure no negative force is added
        Vector3 force = new Vector3(0, altUpFirePower, altFirePower + Mathf.Abs(pm.playerZXVel.magnitude) * forceMultiplier); //need to account for shot direction
        nadeRB.AddRelativeForce(force, ForceMode.Impulse);

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
