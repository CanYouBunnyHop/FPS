using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GunBehaviour : MonoBehaviour
{
    [Header("References")]
    public GunData gunData;
    public Animator anim;
    public GameObject gunModel; //for switching weapon
    [Header("probably should be static")]
    [SerializeField] protected LayerMask enemyMask;
    [SerializeField] protected LayerMask groundMask;
    [SerializeField] protected LayerMask groundEnemyMask;
    [SerializeField] protected PlayerCamera pcam;
    protected Camera cam;
    [SerializeField] protected GameObject bulletHoleFx;
    /////
    //extra for determining if can shoot
    [Header("Debug")]
    [SerializeField]protected bool canShoot;
    [SerializeField]protected float timeSinceLastShot;
    [SerializeField]protected float timeBetweenShots;
    protected Coroutine reload;
    Queue<FireInputActionItem> FireIAIQ;
    [SerializeField]protected int shootTimes = 0;
    private float dX;
    private float dY;
    //[SerializeField]protected bool firing;
    
    /// <summary>
    /// Firemode is Tkey, bool = single fire, !bool = auto fire
    /// </summary>
    protected static Dictionary<GunData.FireMode,bool> FireModeDatas;


    protected void Awake()
    {
        cam = Camera.main;
        gunData.currentAmmo = gunData.magSize;
        gunData.isReloading = false;
        
        FireIAIQ = new Queue<FireInputActionItem>();

        FireModeDatas = new Dictionary<GunData.FireMode, bool>() //initialise static dictionary //bool true is semi auto
        {
            [GunData.FireMode.SemiAuto] = true,
            [GunData.FireMode.BurstFire] = true,

            [GunData.FireMode.FullAuto] = false,
            [GunData.FireMode.Charge] = false,
        };

        timeBetweenShots = 1 / (gunData.fireRate / 60); //fire rate is in rpm, rounds per minute
    }
    #region for manager update and fixedUpdate
    public void BehaviorFixedUpdate()
    {
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
                    if(canShoot)
                    {
                        AltShoot();
                        FireIAIQ.Dequeue(); Debug.Log("dequeue");
                        canShoot = false;
                        timeSinceLastShot = 0;  
                    }
                }
                break;
            }
        }
        
        if(timeSinceLastShot < timeBetweenShots)// recoil
        {
            pcam.Y = Mathf.Lerp(pcam.Y, pcam.Y + dY, gunData.recoilSpeed * Time.deltaTime);
            pcam.X = Mathf.Lerp(pcam.X, pcam.X + dX, gunData.recoilSpeed * Time.deltaTime);
        }
        if(timeSinceLastShot > timeBetweenShots + 0.15f) //recoil rest
        {
            pcam.X = Mathf.SmoothStep(pcam.X, 0,  timeSinceLastShot);
            pcam.Y = Mathf.SmoothStep(pcam.Y, 0,  timeSinceLastShot);  //return to default x pos
            shootTimes = 0;
        }
    }
    
    public void BehaviorInputUpdate()
    {
        //hold or tap
         InputUpdate(FireModeDatas.GetValueOrDefault(gunData.defaultFireMode) ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0), 
                     FireModeDatas.GetValueOrDefault(gunData.altFireMode) ? Input.GetMouseButtonDown(1) : Input.GetMouseButton(1));

        ReloadInput();

        //
        //Debug.Log("THIS IS MOUSE " + mouseInput);
    }
    private void InputUpdate(bool _m0, bool _m1) //the bool is input.getmouse
    {
        //if allow double fire
        if(gunData.allowDoubleFire)
        {
            if(_m0) 
            {
                EnqueueShootInput(gunData.defaultFireMode, 0);
            }
            if(_m1) 
            {
                EnqueueShootInput(gunData.altFireMode, 1);
            }
        }
        else //dont allow double fire
        {
            if(_m0 && !_m1)
            {
                EnqueueShootInput(gunData.defaultFireMode, 0);
            }
            if(_m1 && !_m0)
            {
                EnqueueShootInput(gunData.altFireMode, 1);
            }
        }
    }
    #endregion

    #region  inputs


    /// <summary>
    /// _selectFire is Firemode uses to fire. _fireInput = 0 is normal fire, 1 is alt fire.
    /// </summary>
    protected virtual void EnqueueShootInput(GunData.FireMode _selectFire, int? _fireInput)
    {
        switch(_selectFire)
        {
            case GunData.FireMode.SemiAuto:
            {
                //semi auto fire
                if( gunData.currentAmmo > 0 && timeSinceLastShot > timeBetweenShots - gunData.fireBuffer)//only queue the item if this condition is met
                {
                    FireInputActionItem item = new FireInputActionItem((FireInputActionItem.fireActionItem)_fireInput);
                    FireIAIQ.Enqueue(item); 
                    //Debug.Log("queue");
                    //queue here
                }

            }
            break;

            case GunData.FireMode.FullAuto:
            {
                if( gunData.currentAmmo > 0 && timeSinceLastShot > timeBetweenShots)
                {
                    Shoot();
                    canShoot = false;
                    timeSinceLastShot = 0;
                }

            }
            break;

            case GunData.FireMode.BurstFire:
            break;

            case GunData.FireMode.Charge:
            break;
        }
        
        
    }
    protected virtual void ReloadInput()
    {
        //manual Reload
        if (Input.GetKey(KeyCode.R) && gunData.currentAmmo < gunData.magSize)
        {
            if (!gunData.isReloading)
            {
                reload = StartCoroutine(Reload());
                //animation
                Anim_Reload();
            }
              
        }
        //auto reload
        if ((Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1)) && gunData.currentAmmo <= 0 && timeSinceLastShot > timeBetweenShots-0.2)
        {
            if (!gunData.isReloading)
            {
                reload = StartCoroutine(Reload());
                //animation
                Anim_Reload();
            }
               
        }
        //Cancel reload
        if (Input.GetMouseButtonDown(0)&& !Input.GetMouseButtonDown(1) && gunData.isReloading &&
        gunData.currentAmmo > 0 && gunData.canCancelReloadWithFire)
        {
            CancelReload(reload);
            EnqueueShootInput(gunData.defaultFireMode, 0);
            Debug.Log("reload q");
        }
        if (Input.GetMouseButtonDown(1)&& !Input.GetMouseButtonDown(0) && gunData.isReloading &&
        gunData.currentAmmo > 0 && gunData.canCancelReloadWithAltFire)
        {
            CancelReload(reload);
            EnqueueShootInput(gunData.altFireMode, 1);
            Debug.Log("reload q");
        }
    }
    #endregion
   
   #region shootBehaviors
    //shoot actual behaviors here
    /// <summary>
    /// Shoot behaviors don't include default behaviors, needs to override it and define it, 
    /// base does animation and recoil calculation only
    /// </summary>
    protected virtual void Shoot()
    {
        
        shootTimes+=1;
        dY = gunData.recoilX.Evaluate(shootTimes * 0.1f) * gunData.recoilXScale;
        dX = gunData.recoilY.Evaluate(shootTimes * 0.1f) * gunData.recoilYScale;
        // if(timeSinceLastShot < timeBetweenShots)
        // {
        //     pcam.Y = Mathf.Lerp(pcam.Y, pcam.Y+dY, Time.deltaTime);
        //     pcam.X = Mathf.Lerp(pcam.X, pcam.X+dX, Time.deltaTime);
        // }
       
       Anim_Shoot();
    }
    /// <summary>
    /// Shoot behaviors don't include default behaviors, needs to override it and define it, 
    /// base does animation and recoil calculation only
    /// </summary>
    protected virtual void AltShoot()
    {
       Anim_AltShoot();
    }
    #endregion
    protected void BulletHoleFx(RaycastHit _hit)
    {
        GameObject hole = Instantiate(bulletHoleFx, _hit.point, transform.rotation);
        hole.transform.SetParent(_hit.collider.gameObject.transform);
        //object pooling for the hole fx, to do
    }
    #region reload
    protected virtual IEnumerator Reload()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(gunData.reloadSpeed);
        gunData.isReloading = true;

        yield return wait;
        gunData.isReloading = false;
        gunData.currentAmmo = gunData.magSize;
    }
    // public virtual IEnumerator ReloadOne()
    // {
    //     return null;
    // }
    protected virtual void CancelReload(Coroutine IEReload)
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

    //nested class because currently not using in any other things
    ///<summary>
    ///Class that defines what action the queue will do next.
    ///</summary>
    protected class FireInputActionItem
    {
        public fireActionItem? fireIAI;
        ///<summary>
        ///Item inclues "FireAction", "AltFireAction, index matches MouseButton0, MouseButton1"
        ///</summary>
        public enum fireActionItem
        {
            FireAction,
            AltFireAction,
        }
        public FireInputActionItem(fireActionItem? _input) //CONSTRUCTOR
        {
            fireIAI = _input;
        }
    }
}