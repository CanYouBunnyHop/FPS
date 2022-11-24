using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GunBehavior : MonoBehaviour
{

    public GunData gunData;
    public Animator anim;
    public GameObject gunModel; //for switching weapon
    [SerializeField]
    protected LayerMask enemyMask;
    [SerializeField]
    protected LayerMask groundMask;
    [SerializeField]
    protected LayerMask groundEnemyMask;
    protected Camera cam;
    [SerializeField]
    protected GameObject bulletHoleFx;
    /////
    //extra for determining if can shoot
    public bool canShoot;
    public float timeSinceLastShot;
    public float timeBetweenShots;

    //use these bool to control shooting if gun does not allow simutaneous action
    protected bool startQshoot = false;
    protected bool startQaltShoot = false;
    protected Coroutine reload;

    protected void Awake()
    {
        cam = Camera.main;
        gunData.currentAmmo = gunData.magSize;
        gunData.isReloading = false;
    }
    #region for manager update and fixedUpdate
    public void BehaviorFixedUpdate()
    {
        timeBetweenShots = 1 / (gunData.fireRate / 60); //fire rate is in rpm, rounds per minute

        canShoot = !gunData.isReloading && timeSinceLastShot > timeBetweenShots && gunData.currentAmmo > 0;

        //calc timeSicelastShot
        timeSinceLastShot += Time.deltaTime;
    }
    public virtual void BehaviorInputUpdate()
    {

    }
    #endregion
    #region  inputs
    protected virtual void ShootInput()
    {
        
    }
    protected virtual void AltShootInput()
    {

    }
    protected virtual void ReloadInput()
    {
        //manual Reload
        if (Input.GetKey(KeyCode.R) && gunData.currentAmmo < gunData.magSize)
        {
            if (!gunData.isReloading)
                reload = StartCoroutine(Reload());
            //animation
            Anim_Reload();
        }
        //auto reload
        if ((Input.GetMouseButtonDown(0) && gunData.currentAmmo <= 0 && Time.time > 0.1f)
        || (Input.GetMouseButtonDown(1) && gunData.currentAmmo <= 0 && Time.time > 0.1f))
        {
            if (!gunData.isReloading)
                reload = StartCoroutine(Reload());
            //animation
            Anim_Reload();
        }
        //Cancel reload
        if (Input.GetMouseButtonDown(0) && gunData.isReloading &&
        gunData.currentAmmo > 0 && gunData.canCancelReload)
        {
            CancelReload(reload);
            QueueShoot();
        }
        if (Input.GetMouseButtonDown(1) && gunData.isReloading &&
        gunData.currentAmmo > 0 && gunData.canCancelReload)
        {
            CancelReload(reload);
            QueueAltShoot();
        }
    }
    #endregion
    //may not need, for queueing inputs, depends on guns
    public virtual void QueueShoot()
    {

    }
    public virtual void QueueAltShoot()
    {

    }
    //shoot
    public virtual void Shoot()
    {

    }
    public virtual void AltShoot()
    {

    }
    protected void BulletHoleFx(RaycastHit _hit)
    {
        GameObject hole = Instantiate(bulletHoleFx, _hit.point, transform.rotation);
        hole.transform.SetParent(_hit.collider.gameObject.transform);
        //object pooling for the hole fx, to do
    }
    #region reload
    public virtual IEnumerator Reload()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(gunData.reloadSpeed);
        gunData.isReloading = true;

        yield return wait;
        gunData.isReloading = false;
        gunData.currentAmmo = gunData.magSize;
    }
    public virtual IEnumerator ReloadOne()
    {
        return null;
    }
    public virtual void CancelReload(Coroutine IEReload)
    {
        StopCoroutine(IEReload);
        gunData.isReloading = false;
    }
    #endregion
    //animations
    #region  animations
    public virtual void Anim_Shoot()
    {

    }
    public virtual void Anim_AltShoot()
    {

    }
    public virtual void Anim_Reload()
    {

    }
    #endregion

}
