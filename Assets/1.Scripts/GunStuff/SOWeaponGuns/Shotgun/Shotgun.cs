using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPS.Player;
using Enemy.Movement;

public class Shotgun : GunBehaviour
{
    [Header("Shotgun specific stats")]
    public float selfKnockBack;
    public float enemyKnockBack;
    public float enemyKnockUp;
    [SerializeField] PlayerMovement pm;
    [SerializeField]private List<EnemyMovement> emlist = new List<EnemyMovement>();
    [SerializeField] int pellets;
    [SerializeField] float spread;
    [SerializeField] float resetVelMinDotProduct;
    [SerializeField] bool startKnockBack;
    [SerializeField] GrapplingHook hook;

    #region Input
    protected override void EnqueueShoot_Input(GunDataSO.FireMode _fireMode, int? _fireInput)
    {
        base.EnqueueShoot_Input(_fireMode, _fireInput);
    }
    protected override void Reload_Input()
    {
        base.Reload_Input();
    }
    #endregion

    #region  Shooting Behaviors
    protected override void Shoot()
    {
        //clear list
        emlist.Clear();

        //if (pm.currentState == PlayerMovement.State.GrapSurface ||  pm.currentState == PlayerMovement.State.HookEnemy)
        // if (pm.currentActionSubState is ActionSubState_GrappleSurface or ActionSubState_HookEnemy)
        // {
        //     hook.EndGrapple();
        // }
        
        //ammo
        gunData.currentAmmo--;

        //shooting raycast bullets
        ShootingBehavior();

        PlayerKnockBack();
        StartCoroutine(EnemyKnockBack());

        base.Shoot();
    }
    private void ShootingBehavior()
    {
        for (int x = 0; x < pellets;)
        {
            //calc where to shoot linecast to
            Vector3 hit = cam.transform.position + aimDir * gunData.range;
            float randomSpreadx = Random.Range(-spread, spread);
            float randomSpready = Random.Range(-spread, spread);
            float randomSpreadz = Random.Range(-spread, spread);
            Vector3 newPos = new Vector3(hit.x + randomSpreadx, hit.y + randomSpready, hit.z + randomSpreadz);

            //if linecast hits
            RaycastHit spreadHit;

            if(Physics.Raycast(cam.transform.position, aimDir, out spreadHit, Mathf.Infinity, (enemyMask | groundMask), QueryTriggerInteraction.Ignore))
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
    protected override IEnumerator Reload()
    {
        return base.Reload();
    }
    protected override void CancelReload(Coroutine IEReload)
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

            pm.playerVelocity -= aimDir * selfKnockBack;
        }
    }
    private void StartKnockBackFalse()
    {
        startKnockBack = false;
    }
    private bool CheckDirToResetVel()
    {
        Vector3 vec = new Vector3(pm.playerVelocity.x, 0, pm.playerVelocity.z);
        if (Vector3.Dot(vec.normalized, aimDir) > resetVelMinDotProduct) //same or close direction 
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
            em.enemyVelocity += aimDir * enemyKnockBack;
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
    public override void Anim_SpecialShoot()
    {
        //anim.Play("AltShoot");
    }
    public override void Anim_Reload()
    {
        anim.Play("ReloadingTest");
    }
    #endregion

}
