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
    [SerializeField]
    private GunBehaviour[] gunBehaviors;
    [SerializeField]
    public GunBehaviour currentBehavior;
    [SerializeField]
    
    //Ui
    public Text ammoDisplay;
    public Image altShootIndicator;
   
    [SerializeField]
    private int currentAmmo;
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
    }
    private void FixedUpdate()
    {
       currentBehavior.BehaviorFixedUpdate();
    }

    // Input
    void Update()
    {
        //Switch Gun input
        currentBehavior = SwitchGun();

        //define current ammo
        currentAmmo = currentBehavior.gunData.currentAmmo;

        //UI
        ammoDisplay.text = currentAmmo.ToString() + "/" + currentBehavior.gunData.magSize.ToString();
        altShootIndicator.enabled = currentBehavior.gunData.enableAltShootIndicator;

        //Gun inputs
        currentBehavior.BehaviorInputUpdate();
    }
   
    
    
    private GunBehaviour SwitchGun()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1) && currentBehavior!=gunBehaviors[0])
        {
            try
            {
                GunBehaviour gunToSwitch = gunBehaviors[0];
                //currentBehavior.Anim_Holster
                currentBehavior.gunModel.SetActive(false);
                gunToSwitch.gunModel.SetActive(true);
                //gunToSwitch.Anim_Unholster
                return gunBehaviors[0];
               
                //currentBehavior.Anim_Unholster
            }
            catch(IndexOutOfRangeException)
            {
                Debug.Log("You don't have that many weapons");
                return currentBehavior;
            }
        }
        if(Input.GetKeyDown(KeyCode.Alpha2) && currentBehavior!=gunBehaviors[1])
        {
            try
            {
                GunBehaviour gunToSwitch = gunBehaviors[1];
                //currentBehavior.Anim_Holster
                currentBehavior.gunModel.SetActive(false);
                gunToSwitch.gunModel.SetActive(true);
                //gunToSwitch.Anim_Unholster
                return gunBehaviors[1];
            }
            catch(IndexOutOfRangeException)
            {
                Debug.Log("You don't have that many weapons");
                return currentBehavior;
            }
        }
        if(Input.GetKeyDown(KeyCode.Alpha3) && currentBehavior!=gunBehaviors[2])
        {
            try
            {
                GunBehaviour gunToSwitch = gunBehaviors[2];
                //currentBehavior.Anim_Holster
                currentBehavior.gunModel.SetActive(false);
                gunToSwitch.gunModel.SetActive(true);
                //gunToSwitch.Anim_Unholster
                return gunBehaviors[2];
            }
            catch(IndexOutOfRangeException)
            {
                Debug.Log("You don't have that many weapons");
                return currentBehavior;
            }
        }
        else
        {
            return currentBehavior;
        }
    }
}