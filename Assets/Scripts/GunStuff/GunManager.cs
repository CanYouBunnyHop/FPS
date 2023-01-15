using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
public class GunManager : MonoBehaviour
{
    //-------------------------------------------------------------------------------------
    // Weapon System Setup
    //  1. Scriptable object currentBehavior.gunData
    //  2. Abstract class Gun behavior with virtual methods and a reference to gun data
    //  3. Create Classes derived from the abstract gun behavior, override virtual methods
    //  
    //  currentBehavior.GunData > GunBehavior > Unique Guns
    //-------------------------------------------------------------------------------------
    [SerializeField] private GunBehaviour[] gunBehaviors;
    [SerializeField] public GunBehaviour currentBehavior;
    [SerializeField] private Dictionary<KeyCode, int> keyCodeDic;
    //Ui
    public Text ammoDisplay;
    public Image aSI;
    [SerializeField] private IndicatorManager iM;
    [SerializeField] private int currentAmmo;
    void Awake()
    {
        currentBehavior = gunBehaviors[0];
        //Invoke(nameof(CST), 0.1f);
        //reload = StartCoroutine(currentBehavior.Reload());

        //make sure the right gun model is loaded for the right gun
        for(int x = 0; x < gunBehaviors.Length; x++)
        {
            if(gunBehaviors[x]!=currentBehavior)
            {
                gunBehaviors[x].gunModel.SetActive(false);
            }
            else
            {
                gunBehaviors[x].gunModel.SetActive(true);
            }
        }

        keyCodeDic = new Dictionary<KeyCode,int>()
        {
            [KeyCode.Alpha1] = 0,
            [KeyCode.Alpha2] = 1,
            [KeyCode.Alpha3] = 2,
            [KeyCode.Alpha4] = 3,
        };
    }
    private void FixedUpdate()
    {
       currentBehavior.BehaviorFixedUpdate();
    }

    // Input
    void Update()
    {
        //Switch Gun input
        SwitchGun(out GunBehaviour _gunToSwitch);
        currentBehavior = _gunToSwitch;

        //define current ammo
        currentAmmo = currentBehavior.gunData.currentAmmo;

        //UI
        ammoDisplay.text = currentAmmo.ToString() + "/" + currentBehavior.gunData.magSize.ToString();
        aSI.enabled = currentBehavior.gunData.indicatorType != GunData.AmmoOrCd.NotUsed? true : false;

        //Gun inputs
        currentBehavior.BehaviorInputUpdate();
    }
   
    
    
    private void SwitchGun(out GunBehaviour _gunToSwitch)
    {
        _gunToSwitch = currentBehavior;
        foreach(KeyValuePair<KeyCode, int> key in keyCodeDic)//loop through dictionary to match inputs
        {
            if(Input.GetKeyDown(key.Key) && currentBehavior != gunBehaviors[key.Value]) 
            {
                try //switch weapon
                {
                    GunBehaviour gunToSwitch = gunBehaviors[key.Value];
                    //currentBehavior.Anim_Holster
                    currentBehavior.gunModel.SetActive(false);
                    gunToSwitch.gunModel.SetActive(true);
                    //gunToSwitch.Anim_Unholster
                    _gunToSwitch = gunToSwitch;
                    //currentBehavior.Anim_Unholster
                }
                catch(IndexOutOfRangeException)
                {
                    Debug.Log("You don't have that many weapons");
                    _gunToSwitch = currentBehavior;
                }
            }
        }
    }
}