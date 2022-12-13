using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GunBehaviour : MonoBehaviour
{

    public GunData gunData;
    public Animator anim;
    public GameObject gunModel; //for switching weapon
    [SerializeField] protected LayerMask enemyMask;
    [SerializeField] protected LayerMask groundMask;
    [SerializeField] protected LayerMask groundEnemyMask;
    protected Camera cam;
    [SerializeField] protected GameObject bulletHoleFx;
    /////
    //extra for determining if can shoot
    public bool canShoot;
    public float timeSinceLastShot;
    public float timeBetweenShots;

    //use these bool to control shooting if gun does not allow simutaneous action
    // protected bool startQshoot = false;
    // protected bool startQaltShoot = false;
    protected bool queueingFire;
    public int? mouseInput;
    //protected int? QInput;
    //protected StartQ startQ;
    protected Coroutine reload;
    protected enum FireMode
    {
       SemiAuto,
       FullAuto,
       BurstFire,
       Charge,
    }
    [Header("Default Fire Select")]
    [SerializeField] FireMode defaultFireMode;
    [SerializeField] FireMode altFireMode;
    [SerializeField] Queue<FireInputActionItem> FireIAIQ;
    protected void Awake()
    {
        cam = Camera.main;
        gunData.currentAmmo = gunData.magSize;
        gunData.isReloading = false;
        
        FireIAIQ = new Queue<FireInputActionItem>();
    }
    #region for manager update and fixedUpdate
    public void BehaviorFixedUpdate()
    {
        timeBetweenShots = 1 / (gunData.fireRate / 60); //fire rate is in rpm, rounds per minute

        canShoot = !gunData.isReloading && timeSinceLastShot > timeBetweenShots && gunData.currentAmmo > 0;

        //calc timeSicelastShot
        timeSinceLastShot += Time.deltaTime;

        if(FireIAIQ != null)
        {
            FireInputActionItem action = FireIAIQ.Peek();
            switch(action.fireIAI)
            {
                case FireInputActionItem.fireActionItem.FireAction:
                {
                    Shoot();
                }
                break;
                case FireInputActionItem.fireActionItem.AltFireAction:
                {
                    AltShoot();
                }
                break;
            }
            FireIAIQ.Dequeue();
            
        }
    }
    public virtual void BehaviorInputUpdate()
    {
        if(Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))//if only m1 is pressed
        {
            mouseInput = 0;
        }
        if(Input.GetMouseButtonDown(1)&& !Input.GetMouseButtonDown(0))// if only m2 is pressed
        {
            mouseInput = 1;
        }
        if(Input.GetMouseButtonDown(1)&& Input.GetMouseButtonDown(0))//if both buttons are pressed
        {
            mouseInput = 2;
        }
        if(!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))// if no mouse button is pressed
        {
            mouseInput = null;
        }

        //check mouseinput and if conditions met before shooting //need to store mouse input into a queue, and check queueing fire instead
        // if(queueingFire && canShoot)
        // {
        //     QueueShoot((FireInputActionItem.fireActionItem)mouseInput);
        // }


        //queueing fire inputs
        if(mouseInput == 0 && !queueingFire)
        {
            ShootInput(defaultFireMode, mouseInput);
        }
        if(mouseInput == 1 && !queueingFire)
        {
            ShootInput(altFireMode, mouseInput);
        }
        //AltShootInput(altFireMode);
        ReloadInput();
    }
    #endregion
    #region  inputs
    protected virtual void ShootInput(FireMode _selectFire, int? _fireInput)//bool _startQ) //i dont want a bool here, but a reference to bool
    {
        queueingFire = true;

        switch(_selectFire)
        {
            case FireMode.SemiAuto:
            {
                //semi auto fire
                if( gunData.currentAmmo > 0 
                && timeSinceLastShot > timeBetweenShots - 0.2f)//only queue the item if this condition is met
                {
                    FireInputActionItem item = new FireInputActionItem((FireInputActionItem.fireActionItem)_fireInput);
                    FireIAIQ.Enqueue(item); 
                    //queue here
                }

            }
            break;

            case FireMode.FullAuto:
            break;

            case FireMode.BurstFire:
            break;

            case FireMode.Charge:
            break;
        }
        
        
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
        if (mouseInput!=null && gunData.currentAmmo <= 0 && Time.time > 0.1f)
        {
            if (!gunData.isReloading)
                reload = StartCoroutine(Reload());
            //animation
            Anim_Reload();
        }
        //Cancel reload
        if (mouseInput !=null && gunData.isReloading &&
        gunData.currentAmmo > 0 && gunData.canCancelReload)
        {
            CancelReload(reload);
            ShootInput((FireMode)mouseInput, mouseInput);
        }
    }
    #endregion
    //may not need, for queueing inputs, depends on guns
    // protected virtual void QueueShoot(FireInputActionItem.fireActionItem _fAI)
    // {
    //     if(_fAI == FireInputActionItem.fireActionItem.FireAction)
    //     {
    //          Shoot(); //remember set q shoot bool to false
    //          Anim_Shoot(); //animation
    //     }
       

    //     if(_fAI == FireInputActionItem.fireActionItem.AltFireAction)
    //     {
    //         AltShoot();
    //         Anim_AltShoot();//animation
    //     }

    //     canShoot = false;
    //     timeSinceLastShot = 0;
    // }
    //shoot
    public virtual void Shoot()
    {
        queueingFire = false;
    }
    public virtual void AltShoot()
    {
        queueingFire = false;
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

    //nested class because currently not using in any other things
    protected class FireInputActionItem
    {
        public fireActionItem? fireIAI;
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