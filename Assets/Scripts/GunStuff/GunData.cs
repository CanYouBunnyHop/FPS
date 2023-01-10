using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Guns", menuName = "Weapons/GunData")]
public class GunData : ScriptableObject
{
    public string gunName;
    [Header("Stats")]
    public float damage;
    [Tooltip("Rounds per minute")] public float fireRate; //in RPM
    public float range;

    [Header("Ammo")]
    public int magSize;
    public int currentAmmo;
    

    [Header("altshoot indicator")]
    //public bool enableAltShootIndicator = false;
    public AmmoOrCd indicatorType;
    public enum AmmoOrCd
    {
        NotUsed,
        Ammo,
        Cd,
        Other
    } 

    [Header("Reload")]
    public float reloadSpeed;
    public bool isReloading;
    public bool canCancelReloadWithFire;
    public bool canCancelReloadWithAltFire;
    
    [Header("Recoil")]
    public AnimationCurve recoilHor;
    public AnimationCurve recoilVer;
    public float recoilHorScale = 1;
    public float recoilVerScale = 1;
    public float recoilSpeed;
    public float returnSpeed;
    public float returnDelay;

    [Header("Recoil Randomness")]
    public bool enableRandomness;
    public float horizontalRandomness;

    [Header("Recoil Offset")]
    public AnimationCurve recoilHorOffset;
    public AnimationCurve recoilVerOffset;
    public float horOffSetScale;
    public float verOffSetScale;

    //public AnimationCurve recoilZ;

    [Header("Buffer")]
    public float fireBuffer = 0.2f;

    

    [Header("Select fire")]
    public FireMode defaultFireMode;
    public FireMode altFireMode;
    public bool allowDoubleFire;
    public enum FireMode
    {
       SemiAuto,
       FullAuto,
       BurstFire,
       Charge,
    }
}
