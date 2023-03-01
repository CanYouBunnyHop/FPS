using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GunBehaviour : MonoBehaviour
{
    //Important Note:
    //
    // 1, Middle Click = weapon special ie: Alt Shoot (old def)
    // 2, Right Click = ADS
    // 3, "LAlt" = toggle BackWards Shoot
    //
    [Header("References")]
    public GunDataSO gunData;
    public Animator anim;
    public GameObject gunModel; //for switching weapon
    //protected GunManager gm;

    [Header("probably should be static")]
    [SerializeField] protected LayerMask enemyMask;
    [SerializeField] protected LayerMask groundMask;
    //[SerializeField] protected LayerMask groundEnemyMask; //is there a way to get around this?
    [SerializeField] protected PlayerCamera recoilManager;
    protected Camera cam;
    protected float camDefaultFOV;
    [SerializeField] protected GameObject bulletHoleFx;
    /////
    //extra for determining if can shoot
    [Header("recoil debug")]
    public int shootTimes = 0;
    public float dX {get; protected set;}
    public float dY {get; protected set;}

    

    [Header("reload + queueing fire")]
    protected Coroutine reload;
    public Queue<FireInputActionItem> FireIAIQ;
    
    //[SerializeField]protected bool firing;
    /// <summary>
    /// Firemode is Tkey, bool = single fire, !bool = auto fire
    /// </summary>
    protected static Dictionary<GunDataSO.FireMode,bool> FireModeDatas;

    [Header("Debug")]
    [SerializeField]protected bool canShoot;
    [SerializeField]protected bool canSpecialShoot;
    [SerializeField]public float timeSinceLastShot {get; protected set;}
    [SerializeField]public float timeBetweenShots {get; protected set;}
    [SerializeField]protected Vector3 aimDir;


    protected void Awake()
    {
        cam = Camera.main;
        camDefaultFOV = cam.fieldOfView;

        gunData.backwards = false;

        gunData.currentAmmo = gunData.magSize;
        gunData.isReloading = false;
        
        FireIAIQ = new Queue<FireInputActionItem>();

        FireModeDatas = new Dictionary<GunDataSO.FireMode, bool>() //initialise static dictionary //bool true is semi auto (cant hold down fire)
        {
            [GunDataSO.FireMode.SemiAuto] = true,
            [GunDataSO.FireMode.BurstFire] = true,

            [GunDataSO.FireMode.FullAuto] = false,
            [GunDataSO.FireMode.Charge] = false,
        };

        timeBetweenShots = 1 / (gunData.fireRate / 60); //fire rate is in rpm, rounds per minute
    }
    #region for manager update and fixedUpdate
    public virtual void BehaviorFixedUpdate()
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
                case FireInputActionItem.fireActionItem.SpecialFireAction:
                {
                    
                    if(canSpecialShoot)
                    {
                        SpecialShoot();
                        FireIAIQ.Dequeue(); Debug.Log("dequeue");
                        canSpecialShoot = false;
                        //set cooldown 
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
    }
    
    public void BehaviorInputUpdate()
    {
        //hold or tap
        InputUpdate(FireModeDatas.GetValueOrDefault(gunData.defaultFireMode) ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0), 
                    FireModeDatas.GetValueOrDefault(gunData.specialFireMode) ? Input.GetMouseButtonDown(2) : Input.GetMouseButton(2));

        ReloadInput();

        //
        //Debug.Log("THIS IS MOUSE " + mouseInput);
    }
    private void InputUpdate(bool _m0, bool _m2) //the bool is input.getmouse
    {
        if(_m0) 
        {
            EnqueueShootInput(gunData.defaultFireMode, 0); //left click
        }
        
        if(_m2) 
        {
            EnqueueShootInput(gunData.specialFireMode, 2); //middle click
        }
        
        if(Input.GetKeyDown(KeyCode.LeftAlt))
        {
            gunData.backwards = !gunData.backwards; //toggle aimBackwards
        }

       //cam.fieldOfView = Input.GetMouseButton(1) ? gunData.ADS_fov : camDefaultFOV;
       cam.fieldOfView = Input.GetMouseButton(1) ? Mathf.Lerp(cam.fieldOfView, gunData.ADS_fov, Time.deltaTime * gunData.ADS_speed) : 
                                                   Mathf.Lerp(cam.fieldOfView, camDefaultFOV, Time.deltaTime * gunData.ADS_speed);
    }
    #endregion

    #region  inputs


    /// <summary>
    /// _selectFire is Firemode
    /// </summary>
    protected virtual void EnqueueShootInput(GunDataSO.FireMode _selectFire, int? _fireInput)
    {
        switch(_selectFire)
        {
            case GunDataSO.FireMode.SemiAuto:
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

            case GunDataSO.FireMode.FullAuto:
            {
                if( gunData.currentAmmo > 0 && timeSinceLastShot > timeBetweenShots)
                {
                    Shoot();
                    canShoot = false;
                    timeSinceLastShot = 0;
                }

            }
            break;

            case GunDataSO.FireMode.BurstFire:
            break;

            case GunDataSO.FireMode.Charge:
            break;
        }
    }
    protected virtual void AimDownSights()
    {
        //FOV changes
        //Shooting less spread + recoil
        
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
        if (Input.GetMouseButtonDown(3)&& !Input.GetMouseButtonDown(0) && gunData.isReloading &&
        gunData.currentAmmo > 0 && gunData.canCancelReloadWithAltFire)
        {
            CancelReload(reload);
            //EnqueueShootInput(gunData.altFireMode, 1);
            EnqueueShootInput(gunData.specialFireMode, 2);
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
        //remember dir based on aim backwards bool
        aimDir = gunData.backwards? -cam.transform.forward : cam.transform.forward;
        DefaultRecoilBehavior();

        Anim_Shoot();
    }
    /// <summary>
    /// Shoot behaviors don't include default behaviors, needs to override it and define it, 
    /// base does animation and recoil calculation only
    /// </summary>
    protected virtual void SpecialShoot()
    {
        DefaultRecoilBehavior();

        Anim_SpecialShoot();
    }
    private void DefaultRecoilBehavior()
    {
        shootTimes+=1;
        float percent = (float)shootTimes / gunData.magSize; //time 1 always equals the end
        
        dX = gunData.recoilVer.Evaluate(percent) * gunData.recoilVerScale;
        dY = gunData.recoilHor.Evaluate(percent) * gunData.recoilHorScale;

        float xOffSet = Random.Range(-gunData.recoilVerOffset.Evaluate(percent) * gunData.verOffSetScale, gunData.recoilVerOffset.Evaluate(percent) * gunData.verOffSetScale);
        float yOffSet = Random.Range(-gunData.recoilHorOffset.Evaluate(percent) * gunData.horOffSetScale, gunData.recoilHorOffset.Evaluate(percent) * gunData.horOffSetScale);

        //Gun randomness, (left and right) 
        if(gunData.enableRandomness)
        {
                float randomness = percent * Random.Range(-gunData.horizontalRandomness, gunData.horizontalRandomness);

                recoilManager.targetRot += gunData.backwards? new Vector3(dX + xOffSet ,(dY * randomness) + yOffSet, 0) :
                                                            new Vector3(-dX + xOffSet ,(dY * randomness) + yOffSet, 0); 
        }
        else
        {
                recoilManager.targetRot += gunData.backwards? new Vector3(dX + xOffSet ,(dY) + yOffSet, 0) :
                                                            new Vector3(-dX + xOffSet ,(dY) + yOffSet, 0); 
        }
        
        //maybe use this to add more shake?????
       //cam.DOShakeRotation(0.1f, 1, 1, 0, true, ShakeRandomnessMode.Harmonic);
       //cam.DOShakePosition(0.1f,1,0);
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
    public virtual void Anim_SpecialShoot()
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
    public class FireInputActionItem
    {
        public fireActionItem? fireIAI;
        ///<summary>
        ///Item inclues "FireAction", "SpecialFireAction, index matches MouseButton0, MouseButton1"
        ///</summary>
        public enum fireActionItem
        {
            FireAction = 0,
            SpecialFireAction = 2,
        }
        public FireInputActionItem(fireActionItem? _input) //CONSTRUCTOR
        {
            fireIAI = _input;
        }
    }
}