using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player.Movement;
using Enemy.Movement;

public class Shotgun : GunBehaviour
{
    public float selfKnockBack;
    public float enemyKnockBack;
    public float enemyKnockUp;
    public PlayerMovement pm;
    [SerializeField]
    private List<EnemyMovement> emlist = new List<EnemyMovement>();
    private List<EnemyMovement> emKnockBack = new List<EnemyMovement>();
    public int pellets;
    public float spread;
    public float resetVelMinDotProduct;
   // [SerializeField]
    //private GameObject bulletHoleFx;
    private bool startKnockBack;
    [SerializeField]
    private GrapplingHook hook;
    [SerializeField]
    private Vector3 dir;
    //calc appropriate force to add depeding on player velocity and camera dir
    // private float dot;
    //private float forceMultiply;
    //clamp pm vel
    //private float clampMagnitude;
    


    #region  Shooting Behaviors
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
    public override void Shoot()
    {
        //remember dir
        dir = cam.transform.forward;

        //clear list
        emlist.Clear();

        if (pm.currentState == PlayerMovement.State.GrapSurface 
        ||  pm.currentState == PlayerMovement.State.HookEnemy)
        {
            hook.EndGrapple();
        }

        //ammo
        gunData.currentAmmo--;

        //shooting raycast bullets
        ShootingBehavior(); //because of similar behavior between shoot and altshoot, so condensed it

        PlayerKnockBack();
        StartCoroutine(EnemyKnockBack());
    }
    public override void AltShoot()
    {
        //remember dir
        dir = -cam.transform.forward;

        //clear list
        emlist.Clear();

        if (pm.currentState == PlayerMovement.State.GrapSurface || pm.currentState == PlayerMovement.State.HookEnemy)
        {
            hook.EndGrapple();
        }

        //ammo
        gunData.currentAmmo--;

        //shooting raycast bullets
        ShootingBehavior();

        PlayerKnockBack();
        StartCoroutine(EnemyKnockBack());
    }
    // public override void QueueShoot(int? _fireInput)
    // {
    //     base.QueueShoot(_fireInput);
    // }
    

    private void ShootingBehavior()
    {
        for (int x = 0; x < pellets;)
        {
            //calc where to shoot linecast to
            Vector3 hit = cam.transform.position + dir * gunData.range;
            float randomSpreadx = Random.Range(-spread, spread);
            float randomSpready = Random.Range(-spread, spread);
            float randomSpreadz = Random.Range(-spread, spread);
            Vector3 newPos = new Vector3(hit.x + randomSpreadx, hit.y + randomSpready, hit.z + randomSpreadz);

            //if linecast hits
            RaycastHit spreadHit;
            if (Physics.Linecast(cam.transform.position, newPos, out spreadHit, groundEnemyMask, QueryTriggerInteraction.Ignore))
            {
                BulletHoleFx(spreadHit);

                //add enemy movement to list
                if (spreadHit.collider.tag == "Enemy")
                {
                    EnemyMovement enemyHit = spreadHit.collider.GetComponent<EnemyMovement>();
                    emlist.Add(enemyHit);
                }
            }
            x++;
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
    public void FixedUpdate()
    {
        if (startKnockBack)
        {
            // //fix interaction where player is on slope
            pm.stepSinceKockback = 0;

            pm.playerVelocity -= dir * selfKnockBack;
        }
    }
    private void StartKnockBackFalse()
    {
        startKnockBack = false;
    }
    private bool CheckDirToResetVel()
    {
        Vector3 vec = new Vector3(pm.playerVelocity.x, 0, pm.playerVelocity.z);
        if (Vector3.Dot(vec.normalized, dir) > resetVelMinDotProduct) //same or close direction 
        {
           // Debug.Log("Reseted vel");
            return true;
        }
        else return false;
    }
    

    #region  Knockback calc in FixedUpdate
    //handles everthing the gun manipulates palayer velocity
    private void PlayerKnockBack()
    {

        //reset player velocity for better control over movement
        pm.playerVelocity.y = 0;
        if (CheckDirToResetVel())
        {
            pm.playerVelocity.x = 0;
            pm.playerVelocity.z = 0;
        }

        //handles physics calc in fixed update
        startKnockBack = true;
        Invoke(nameof(StartKnockBackFalse), 0.1f);
    }
    private IEnumerator EnemyKnockBack()
    {
        foreach(EnemyMovement em in emlist)
        {
            em.canResetVel = true;
            em.agentUpdatePos = false;
            em.enemyVelocity += dir * enemyKnockBack;
            em.enemyVelocity += Vector3.up * enemyKnockUp;
        }
        yield return null;
        emlist.Clear();
    }
    #endregion
    //animations
    #region Animations
    public override void Anim_Shoot()
    {
        anim.Play("Shoot");
    }
    public override void Anim_AltShoot()
    {
        anim.Play("AltShoot");
    }
    public override void Anim_Reload()
    {
        anim.Play("ReloadingTest");
    }
    #endregion

}
