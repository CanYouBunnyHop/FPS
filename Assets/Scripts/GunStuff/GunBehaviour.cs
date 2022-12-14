using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GunBehaviour : MonoBehaviour
{
    [Header("References")]
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
    [Header("Debug")]
    [SerializeField]protected bool canShoot;
    [SerializeField]protected float timeSinceLastShot;
    [SerializeField]protected float timeBetweenShots;
    public int? mouseInput;
    protected Coroutine reload;
    /////////////////////////////////////////////////////
    protected enum FireMode
    {
       SemiAuto,
       FullAuto,
       BurstFire,
       Charge,
    }
    /////////////////////////////////////////////////////
    [Header("Default Fire Select")]
    [SerializeField] FireMode defaultFireMode;
    [SerializeField] FireMode altFireMode;
    Queue<FireInputActionItem> FireIAIQ;
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

        if(FireIAIQ.Count > 0 && canShoot) //if there are action items in queue
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
           FireIAIQ.Dequeue(); Debug.Log("dequeue");
           canShoot = false;
           timeSinceLastShot = 0;  
           
        }
    }
    public virtual void BehaviorInputUpdate()
    {
        //get tap mouse input (only mouse 0 for now, need better code)
        switch(defaultFireMode)
        {
            case FireMode.SemiAuto or FireMode.BurstFire: //single press, holding is not allowed
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
                //
                if(!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))// if no mouse button is pressed
                {
                    mouseInput = null;
                }
            }
            break;
            

           case FireMode.FullAuto or FireMode.Charge: // holding is allowed
            {
                if(Input.GetMouseButton(0) && !Input.GetMouseButton(1))//if only m1 is pressed/held
                {
                    mouseInput = 0;
                }
                if(Input.GetMouseButton(1)&& !Input.GetMouseButton(0))// if only m2 is pressed/held
                {
                    mouseInput = 1;
                }
                if(Input.GetMouseButton(1)&& Input.GetMouseButton(0))//if both buttons are pressed/held
                {
                    mouseInput = 2;
                }
            }
            break;
        }
           

      
       

        //queueing fire inputs
        if(mouseInput == 0)
        {
            ShootInput(defaultFireMode, mouseInput);
        }
        if(mouseInput == 1)
        {
            ShootInput(altFireMode, mouseInput);
        }
        ReloadInput();

        //
        //Debug.Log("THIS IS MOUSE " + mouseInput);
    }
    #endregion

    #region  inputs
    protected virtual void ShootInput(FireMode _selectFire, int? _fireInput)
    {
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
                    Debug.Log("queue");
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
   
   #region shootBehaviors
    //shoot actual behaviors here
    public virtual void Shoot()
    {
       Anim_Shoot();
    }
    public virtual void AltShoot()
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