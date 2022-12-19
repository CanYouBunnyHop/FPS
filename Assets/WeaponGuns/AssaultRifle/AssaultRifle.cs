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

    //[Header("Debug if double fire is allowed")]
    // [SerializeField]protected bool canAltShoot; //not used unless doublefire is true
    // [SerializeField]public float timeSinceLastAltShot;
    // [SerializeField]public float timeBetweenAltShots;

    public override void BehaviorFixedUpdate()
    {
        // canAltShoot = !gunData.isReloading && timeSinceLastAltShot > timeBetweenAltShots;
        // timeSinceLastAltShot += Time.deltaTime;
        //base.BehaviorFixedUpdate();
        ARgrenadeData.CoolingDown();

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
                        FireIAIQ.Dequeue(); Debug.Log("dequeue");
                        ARgrenadeData.canUseAbility = false;
                        ARgrenadeData.InitiateCoolDown();
                    }
                }
                break;
            }
        }
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
